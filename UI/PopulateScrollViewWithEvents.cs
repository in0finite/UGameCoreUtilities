using System.Linq;
using TMPro;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class PopulateScrollViewWithEvents : MonoBehaviour, IStatsCollectable
	{
		public	float	timeToRemoveEvent = 8;
		public	int		maxNumEvents = 5;

        public bool destroyObjectsWhenRemoving = true;
        public bool disableObjectsWhenRemoving = false;

        public bool clearContentOnStart = false;

		public bool logEventsToConsole = false;

		public DelayedActionInvoker delayedActionInvoker;

        [SerializeField]	private	Transform	m_content = null ;
        public Transform ContentParent => m_content;
		[SerializeField]	private	GameObject	m_eventPrefab = null ;

        readonly HashSetAndList<GameObject> m_addedObjects = new();
        public int NumAddedObjects => m_addedObjects.Count;

        public event System.Action<GameObject> OnRemoveObject;


        PopulateScrollViewWithEvents()
		{
			EditorApplicationEvents.Register(this);
		}

        void Start()
        {
            this.RefreshListOfAddedObjects();

            if (this.clearContentOnStart && Application.isPlaying)
				this.RemoveAllEventsFromUI();
        }

        void RefreshListOfAddedObjects()
        {
            m_addedObjects.Clear();

            if (null == m_content)
                return;

            m_addedObjects.AddRange(m_content.GetFirstLevelChildren().Select(tr => tr.gameObject));
        }

        void RemoveUnwantedObjectsFromList()
        {
            m_addedObjects.RemoveAll(static item => null == item);
        }

        public void EventHappened(string eventText)
		{
            GameObject newGo = this.CreateGameObjectForEvent(eventText);
			this.EventHappened(newGo, eventText);
		}

        public void EventHappened(GameObject eventGo, string eventText = null)
		{
            if (null == eventGo)
                throw new System.ArgumentNullException();

            if (!eventGo.activeSelf) // TODO: remove this check
                throw new System.ArgumentException("Event GameObject must be active, otherwise it will be removed from internal list");

            if (m_addedObjects.Contains(eventGo))
                throw new System.ArgumentException($"Specified GameObject already added: {eventGo.name}");

            UnityEngine.Profiling.Profiler.BeginSample("Populator event happened");

            // TODO: use diff
            this.RemoveUnwantedObjectsFromList();

            if (this.logEventsToConsole)
                Debug.Log(eventText ?? eventGo.name, eventGo);

            eventGo.transform.SetParent(m_content, true);

            m_addedObjects.Add(eventGo);

            if (m_addedObjects.Count > this.maxNumEvents && m_addedObjects.Count > 0)
            {
                // too many events
                // remove 1 event from top
                this.RemoveTopEventFromUIInternal();
            }

            this.delayedActionInvoker.RunOutsideOfCurrentPeriod(this.timeToRemoveEvent, () =>
            {
                if (eventGo != null)
                {
                    this.RemoveEventFromUI(eventGo);
                }
            });

            UnityEngine.Profiling.Profiler.EndSample();
        }

        GameObject CreateGameObjectForEvent(string eventText)
		{
			GameObject go = Instantiate(m_eventPrefab);

            var textMeshPro = go.GetComponentInChildren<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = eventText;
                return go;
            }

            var textComp = go.GetComponentInChildren<UnityEngine.UI.Text>();
			if (textComp != null)
            {
                textComp.text = eventText;
				return go;
            }

			throw new System.InvalidOperationException($"Failed to find {nameof(TextMeshProUGUI)} or {nameof(UnityEngine.UI.Text)} component on prefab");
		}

        void RemoveEventFromUI(GameObject go)
        {
            if (!m_addedObjects.Contains(go))
                return;

            Debug.Log($"Removing event: {go.name}", go);

            m_addedObjects.Remove(go);

            this.OnRemoveObject?.Invoke(go);

            if (this.destroyObjectsWhenRemoving)
                go.DestroyEvenInEditMode();
            else if (this.disableObjectsWhenRemoving)
                go.SetActive(false);
        }

		public void RemoveTopEventFromUI()
        {
            this.RemoveUnwantedObjectsFromList();

            if (m_addedObjects.Count == 0)
                throw new System.ArgumentException("No elements in UI");

            this.RemoveTopEventFromUIInternal();
		}

        void RemoveTopEventFromUIInternal()
        {
            GameObject go = m_addedObjects.GetFirst();
            Debug.Log($"RemoveTopEventFromUIInternal(), removing event: {go.name}", go);
            this.RemoveEventFromUI(go);
        }

        public void RemoveAllEventsFromUI()
        {
            this.RemoveUnwantedObjectsFromList();

            foreach (GameObject item in m_addedObjects.ToArray())
                this.RemoveEventFromUI(item);

            m_addedObjects.Clear();
        }

        public int GetNumEventsInUI()
        {
            this.RemoveUnwantedObjectsFromList();
            return m_addedObjects.Count;
        }
	}
}

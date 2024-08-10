using TMPro;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class PopulateScrollViewWithEvents : MonoBehaviour
	{
		public	float	timeToRemoveEvent = 8;
		public	int		maxNumEvents = 5;

		public bool clearContentOnStart = false;

		public bool logEventsToConsole = false;

		public DelayedActionInvoker delayedActionInvoker;

        [SerializeField]	private	Transform	m_content = null ;
		[SerializeField]	private	GameObject	m_eventPrefab = null ;


        PopulateScrollViewWithEvents()
		{
			EditorApplicationEvents.Register(this);
		}

        void Start()
        {
			if (this.clearContentOnStart && Application.isPlaying)
				this.RemoveAllEventsFromUI();
        }

        public void EventHappened(string eventText)
		{
            GameObject newGo = this.CreateGameObjectForEvent(eventText);
			this.EventHappened(newGo, eventText);
		}

        public void EventHappened(GameObject eventGo, string eventText = null)
		{
            if (this.logEventsToConsole)
                Debug.Log(eventText ?? eventGo.name, this);

            eventGo.transform.SetParent(m_content, false);

            if (this.GetNumEventsInUI() > this.maxNumEvents)
            {
                // too many events
                // remove 1 event
                this.RemoveTopEventFromUI();
            }

            this.delayedActionInvoker.RunOutsideOfCurrentPeriod(this.timeToRemoveEvent, () =>
            {
                if (eventGo != null)
                    eventGo.DestroyEvenInEditMode();
            });
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

		public	void	RemoveTopEventFromUI() {

			if (null == m_content)
				return;
			if (0 == GetNumEventsInUI ())
				return;

			var child = m_content.GetChild (0);
			F.DestroyEvenInEditMode (child.gameObject);

		}

		public	void	RemoveAllEventsFromUI() {

			if (null == m_content)
				return;

			for (int i = 0; i < m_content.childCount; i++) {
				var child = m_content.GetChild (i);
				child.gameObject.DestroyEvenInEditMode();
			}

		}

		public	int		GetNumEventsInUI() {

			if (null == m_content)
				return 0;

			return m_content.childCount;
		}


	}

}

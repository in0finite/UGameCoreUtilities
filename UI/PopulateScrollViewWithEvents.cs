using UnityEngine;

namespace UGameCore.Utilities
{
	public class PopulateScrollViewWithEvents : MonoBehaviour
	{
		public	float	timeToRemoveEvent = 8 ;
		public	int		maxNumEvents = 5 ;

		public DelayedActionInvoker delayedActionInvoker;

        [SerializeField]	private	Transform	m_content = null ;
		[SerializeField]	private	GameObject	m_eventPrefab = null ;


        PopulateScrollViewWithEvents()
		{
			EditorApplicationEvents.Register(this);
		}

        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
        }

		public void EventHappened(string eventText)
		{
            GameObject newGo = this.AddEventToUI(eventText);

			if (this.GetNumEventsInUI() > this.maxNumEvents) {
				// too many events
				// remove 1 event
				this.RemoveTopEventFromUI();
			}

			this.delayedActionInvoker.RunOutsideOfCurrentPeriod(this.timeToRemoveEvent, () =>
			{
				if (newGo != null)
					F.DestroyEvenInEditMode(newGo);
			});
		}

		GameObject	AddEventToUI( string eventText ) {

			if (null == m_eventPrefab)
				return null;

			var go = Instantiate( m_eventPrefab );
			go.transform.SetParent (m_content, false);
			var text = go.GetComponentInChildren<UnityEngine.UI.Text> ();
			text.text = eventText ;

			return go;
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
				F.DestroyEvenInEditMode (child.gameObject);
			}

		}

		public	int		GetNumEventsInUI() {

			if (null == m_content)
				return 0;

			return m_content.childCount;
		}


	}

}

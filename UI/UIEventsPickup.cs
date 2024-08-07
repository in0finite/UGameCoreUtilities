using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{

	public class UIEventsPickup : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
		IPointerUpHandler, IDragHandler
	{

		public	event	Action<PointerEventData>	onPointerClick = delegate {};
		public	event	Action<PointerEventData>	onPointerDoubleClick = delegate {};
        public event Action<PointerEventData> onLeftPointerDoubleClick;
        public event Action<PointerEventData> onLeftPointerClick = delegate { };
        public	event	Action<PointerEventData>	onPointerEnter = delegate {};
		public	event	Action<PointerEventData>	onPointerExit = delegate {};
		public	event	Action<PointerEventData>	onPointerDown = delegate {};
		public	event	Action<PointerEventData>	onPointerUp = delegate {};
		public	event	Action<PointerEventData>	onDrag = delegate {};

		public bool IsPointerInside { get; private set; } = false;
		public bool IsPointerDown { get; private set; } = false;

		public bool IsWaitingForDoubleClick { get; private set; } = false;
		public double TimeWhenClicked { get; private set; } = double.NegativeInfinity;
		public float doubleClickDuration = 0.5f;



		void OnDisable()
		{
			this.IsPointerInside = false;
			this.IsPointerDown = false;
			this.IsWaitingForDoubleClick = false;
        }

		public void OnPointerClick (PointerEventData eventData)
		{
			bool isLeftPointer = eventData.button == PointerEventData.InputButton.Left;
			double timeNow = Time.realtimeSinceStartupAsDouble; // works in edit-mode also

            double oldTimeWhenClicked = this.TimeWhenClicked;
            this.TimeWhenClicked = timeNow;

            if (this.IsWaitingForDoubleClick)
			{
				if (timeNow - oldTimeWhenClicked < this.doubleClickDuration)
				{
                    this.IsWaitingForDoubleClick = false;
                    onPointerDoubleClick(eventData);
					if (isLeftPointer)
						this.onLeftPointerDoubleClick?.Invoke(eventData);
                }
            }
			else
			{
                this.IsWaitingForDoubleClick = true;
            }

            onPointerClick (eventData);

			if (isLeftPointer)
				onLeftPointerClick(eventData);
        }

		public void OnPointerEnter (PointerEventData eventData)
		{
			this.IsPointerInside = true;
			onPointerEnter (eventData);
		}

		public void OnPointerExit (PointerEventData eventData)
		{
			this.IsPointerInside = false;
			onPointerExit (eventData);
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			this.IsPointerDown = true;
			onPointerDown (eventData);
		}

		public void OnPointerUp (PointerEventData eventData)
		{
			this.IsPointerDown = false;
			onPointerUp (eventData);
		}

		public void OnDrag (PointerEventData eventData)
		{
			onDrag (eventData);
		}


	}

}

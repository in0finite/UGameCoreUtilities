using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{
	public class UIEventsPickup : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
		IPointerUpHandler, IDragHandler
	{
		public Action<PointerEventData>	onPointerClick = delegate {};
		public Action<PointerEventData>	onPointerDoubleClick = delegate {};
        public Action<PointerEventData> onLeftPointerDoubleClick;
        public Action<PointerEventData> onLeftPointerClick = delegate { };
        public Action<PointerEventData> onPointerEnter = delegate { };
		public Action<PointerEventData> onPointerExit = delegate { };
		public Action<PointerEventData> onPointerDown = delegate { };
		public Action<PointerEventData> onPointerUp = delegate { };
		public Action<PointerEventData> onDrag = delegate { };

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

		public void ClearAllEvents()
		{
			onPointerClick = null;
			onPointerDoubleClick = null;
			onLeftPointerDoubleClick = null;
			onLeftPointerClick = null;
			onPointerEnter = null;
			onPointerExit = null;
			onPointerDown = null;
			onPointerUp = null;
			onDrag = null;
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
                    onPointerDoubleClick?.Invoke(eventData);
					if (isLeftPointer)
						this.onLeftPointerDoubleClick?.Invoke(eventData);
                }
            }
			else
			{
                this.IsWaitingForDoubleClick = true;
            }

            onPointerClick?.Invoke(eventData);

			if (isLeftPointer)
				onLeftPointerClick?.Invoke(eventData);
        }

		public void OnPointerEnter (PointerEventData eventData)
		{
			this.IsPointerInside = true;
			onPointerEnter?.Invoke(eventData);
		}

		public void OnPointerExit (PointerEventData eventData)
		{
			this.IsPointerInside = false;
			onPointerExit?.Invoke(eventData);
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			this.IsPointerDown = true;
			onPointerDown?.Invoke(eventData);
		}

		public void OnPointerUp (PointerEventData eventData)
		{
			this.IsPointerDown = false;
			onPointerUp?.Invoke(eventData);
		}

		public void OnDrag (PointerEventData eventData)
		{
			onDrag?.Invoke(eventData);
		}
	}
}

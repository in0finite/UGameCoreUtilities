using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{
    public class UIDraggable : MonoBehaviour, IDragHandler
    {
        public bool updateAnchors = true;
        public bool updateOffset = false;
        public bool updateAnchoredPosition = false;

        public Transform transformToDrag = null;


        public void OnDrag(PointerEventData eventData)
        {
            Transform t = this.transformToDrag != null ? this.transformToDrag : this.transform;

            var rt = (RectTransform)t;
            var parent = (RectTransform)t.parent;

            if (this.updateOffset)
            {
                rt.offsetMin += eventData.delta;
                rt.offsetMax += eventData.delta;
            }

            if (this.updateAnchors)
            {
                Vector2 scaledDelta = eventData.delta;
                scaledDelta.x /= parent.rect.width;
                scaledDelta.y /= parent.rect.height;
                rt.anchorMin += scaledDelta;
                rt.anchorMax += scaledDelta;
            }

            if (this.updateAnchoredPosition)
            {
                rt.anchoredPosition += eventData.delta;
            }
        }
    }
}

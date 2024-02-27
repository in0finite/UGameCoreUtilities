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

            var canvas = this.GetComponentInParent<Canvas>();
            Vector2 scaleFactor = canvas != null ? canvas.transform.localScale : Vector2.one;

            Vector2 finalDelta = eventData.delta / scaleFactor;

            if (this.updateOffset)
            {
                rt.offsetMin += finalDelta;
                rt.offsetMax += finalDelta;
            }

            if (this.updateAnchors)
            {
                Vector2 scaledDelta = finalDelta;
                scaledDelta.x /= parent.rect.width;
                scaledDelta.y /= parent.rect.height;
                rt.anchorMin += scaledDelta;
                rt.anchorMax += scaledDelta;
            }

            if (this.updateAnchoredPosition)
            {
                rt.anchoredPosition += finalDelta;
            }
        }
    }
}

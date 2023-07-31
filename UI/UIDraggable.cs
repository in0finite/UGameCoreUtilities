using UnityEngine;
using UnityEngine.EventSystems;

namespace uGameCore.Utilities
{
    public class UIDraggable : MonoBehaviour, IDragHandler
    {
        public bool updateAnchors = true;
        public bool updateOffset = false;
        public bool updateAnchoredPosition = false;


        public void OnDrag(PointerEventData eventData)
        {
            var rt = (RectTransform)this.transform;
            var parent = (RectTransform)this.transform.parent;

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

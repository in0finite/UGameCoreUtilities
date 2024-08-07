using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{
    public class BringToFrontWhenClicked : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this.transform.SetAsLastSibling();
        }
    }
}

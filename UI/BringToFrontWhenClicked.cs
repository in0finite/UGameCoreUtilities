using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{
    public class BringToFrontWhenClicked : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this.transform.SetAsLastSibling();
        }
    }
}

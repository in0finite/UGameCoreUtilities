using UnityEngine;
using UnityEngine.EventSystems;

namespace UGameCore.Utilities
{
    public class UnselectWhenClicked : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (null == EventSystem.current)
                return;

            if (EventSystem.current.currentSelectedGameObject != this.gameObject)
                return;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}

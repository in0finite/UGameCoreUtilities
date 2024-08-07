using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Makes <see cref="Button"/> a <see cref="ILayoutElement"/> by retrieving layout properties 
    /// from it's <see cref="TextMeshProUGUI"/> or <see cref="Text"/> component.
    /// </summary>
    public class ButtonLayoutElement : RedirectedLayoutElement
    {
        ButtonLayoutElement()
        {
            // change default values of some properties
            this.extraWidth = 4;
            this.extraHeight = 4;
        }

        protected override Component GetRedirectedLayoutElement()
        {
            if (!this.gameObject.TryGetComponent(out Button _))
                return null;

            Transform child = this.transform.GetChild(0);
            if (null == child)
                return null;

            if (child.TryGetComponent(out TextMeshProUGUI textMeshPro))
                return textMeshPro;

            if (child.TryGetComponent(out Text text))
                return text;

            return null;
        }
    }
}

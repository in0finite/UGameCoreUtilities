using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    /// <summary>
	/// <see cref="ILayoutElement"/> that can use layout properties from child's <see cref="ILayoutElement"/>.
	/// </summary>
    public class RedirectedChildLayoutElement : RedirectedLayoutElement
    {
        protected override Component GetRedirectedLayoutElement()
        {
            // this could be a hot path, try not to allocate memory here
            Transform tr = this.transform;
            int childCount = tr.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = tr.GetChild(i);
                ILayoutElement layoutElement = child.GetComponentInChildren<ILayoutElement>();
                if (layoutElement != null)
                    return (Component)layoutElement;
            }
            return null;
        }
    }
}

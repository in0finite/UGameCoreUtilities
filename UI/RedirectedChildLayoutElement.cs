using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    /// <summary>
	/// <see cref="ILayoutElement"/> that can use layout properties from child's <see cref="ILayoutElement"/>.
	/// </summary>
    public class RedirectedChildLayoutElement : MonoBehaviour, ILayoutElement
    {
        ILayoutElement m_redirectedLayoutElement
        {
            get
            {
                // this could be a hot path, try not to allocate memory here
                int childCount = this.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = this.transform.GetChild(i);
                    ILayoutElement layoutElement = child.GetComponentInChildren<ILayoutElement>();
                    if (layoutElement != null)
                        return layoutElement;
                }
                return null;
            }
        }

        public void CalculateLayoutInputHorizontal()
        {
            m_redirectedLayoutElement?.CalculateLayoutInputHorizontal();
        }

        public void CalculateLayoutInputVertical()
        {
            m_redirectedLayoutElement?.CalculateLayoutInputVertical();
        }

        public float minWidth => m_redirectedLayoutElement?.minWidth ?? 0f;

        public float preferredWidth => m_redirectedLayoutElement?.preferredWidth ?? 0f;

        public float flexibleWidth => m_redirectedLayoutElement?.flexibleWidth ?? 0f;

        public float minHeight => m_redirectedLayoutElement?.minHeight ?? 0f;

        public float preferredHeight => m_redirectedLayoutElement?.preferredHeight ?? 0f;

        public float flexibleHeight => m_redirectedLayoutElement?.flexibleHeight ?? 0f;

        public int layoutPriority => m_redirectedLayoutElement?.layoutPriority ?? 0;
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    /// <summary>
    /// <see cref="ILayoutElement"/> that can use layout properties from another object.
    /// </summary>
    [DisallowMultipleComponent]
    public class RedirectedLayoutElement : MonoBehaviour, ILayoutElement
	{
		public RectTransform layoutObject = null;

        public int extraWidth = 0;
        public int extraHeight = 0;

        public bool overridePriority = false;
        public int priority = 1;

        ILayoutElement m_redirectedLayoutElement => m_cachedComponent != null // always check if Component is alive
            ? m_cachedLayoutComponent
            : this.GetCachedComponent();

        [System.NonSerialized] Component m_cachedComponent;
        ILayoutElement m_cachedLayoutComponent;
        readonly EmptyLayoutElement m_emptyLayoutElement = new();


        ILayoutElement GetCachedComponent()
        {
            Component c = this.GetRedirectedLayoutElement();

            if (null == c)
            {
                this.ClearCachedLayoutElement();
                return m_emptyLayoutElement;
            }

            m_cachedLayoutComponent = (ILayoutElement)c;
            m_cachedComponent = c;
            return m_cachedLayoutComponent;
        }

        protected virtual Component GetRedirectedLayoutElement()
        {
            return this.layoutObject != null ? (Component)this.layoutObject.GetComponent<ILayoutElement>() : null;
        }

        [ContextMenu("Clear cached layout element")]
        void ClearCachedLayoutElement()
        {
            m_cachedComponent = null;
            m_cachedLayoutComponent = null;
        }

        void OnValidate()
        {
            // mark layout for rebuild
            // code taken from LayoutElement.SetDirty() function
            if (this.isActiveAndEnabled)
            {
                this.RebuildLayout();
            }
        }

        void RebuildLayout()
        {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)this.transform);
        }

        public void CalculateLayoutInputHorizontal ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputHorizontal ();
		}

		public void CalculateLayoutInputVertical ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputVertical ();
		}

        public float minWidth => m_redirectedLayoutElement.minWidth;

        public float preferredWidth => m_redirectedLayoutElement.preferredWidth + this.extraWidth;

        public float flexibleWidth => m_redirectedLayoutElement.flexibleWidth;

        public float minHeight => m_redirectedLayoutElement.minHeight;

        public float preferredHeight => m_redirectedLayoutElement.preferredHeight + this.extraHeight;

        public float flexibleHeight => m_redirectedLayoutElement.flexibleHeight;

        public int layoutPriority => this.overridePriority ? this.priority : m_redirectedLayoutElement.layoutPriority;
    }
}

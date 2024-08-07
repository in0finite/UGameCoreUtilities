using UnityEngine.UI;

namespace UGameCore.Utilities
{
    public class EmptyLayoutElement : ILayoutElement
    {
        public float minWidth => 0f;

        public float preferredWidth => 0f;

        public float flexibleWidth => 0f;

        public float minHeight => 0f;

        public float preferredHeight => 0f;

        public float flexibleHeight => 0f;

        public int layoutPriority => -1;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }
    }
}

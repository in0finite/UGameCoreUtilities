using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// 2D progress bar achieved by changing scale of <see cref="RectTransform"/>.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] RectTransform m_rectTransform = null;
        [Range(0, 1)] [SerializeField] float m_fillAmount = 0;
        [Range(0, 1)] public float minProgress = 0.02f;
        [Range(0, 1)] public float maxProgress = 1f;

        
        public float FillAmount
        {
            get => m_fillAmount;
            set
            {
                m_fillAmount = value;
                this.Refresh();
            }
        }

        void Refresh()
        {
            m_fillAmount = Mathf.Clamp(m_fillAmount, this.minProgress, this.maxProgress);

            if (null == m_rectTransform)
                return;

            Vector3 localScale = m_rectTransform.localScale;
            localScale.x = m_fillAmount;
            m_rectTransform.localScale = localScale;
        }

        private void Awake()
        {
            this.Refresh();
        }

        private void OnValidate()
        {
            this.Refresh();
        }
    }
}

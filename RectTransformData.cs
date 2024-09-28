using System;
using UnityEngine;

namespace UGameCore.Utilities
{
    [Serializable]
    public struct RectTransformData
    {
        public Vector3 AnchoredPosition;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Size;
        public Vector2 Pivot;
        public Quaternion Rotation;
        public Vector3 Scale;

        static readonly RectTransformData s_Default = new RectTransformData().WithDefault();
        public static RectTransformData Default => s_Default;


        public RectTransformData(RectTransform rectTransform)
        {
            this.AnchoredPosition = rectTransform.anchoredPosition3D;
            this.AnchorMin = rectTransform.anchorMin;
            this.AnchorMax = rectTransform.anchorMax;
            this.Size = rectTransform.sizeDelta;
            this.Pivot = rectTransform.pivot;
            this.Rotation = rectTransform.localRotation;
            this.Scale = rectTransform.localScale;
        }

        public void SetDefault()
        {
            this.AnchoredPosition = Vector3.zero;
            this.AnchorMin = Vector2.one * 0.5f;
            this.AnchorMax = Vector2.one * 0.5f;
            this.Size = Vector2.zero;
            this.Pivot = Vector2.one * 0.5f;
            this.Rotation = Quaternion.identity;
            this.Scale = Vector3.one;
        }

        public RectTransformData WithDefault()
        {
            this.SetDefault();
            return this;
        }

        public RectTransformData WithSize(Vector2 size)
        {
            this.Size = size;
            return this;
        }

        public RectTransformData WithSize(float size)
        {
            this.Size = new Vector2(size, size);
            return this;
        }

        public RectTransformData WithPivot(Vector2 pivot)
        {
            this.Pivot = pivot;
            return this;
        }

        public readonly void Apply(RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = AnchoredPosition;
            rectTransform.anchorMin = AnchorMin;
            rectTransform.anchorMax = AnchorMax;
            rectTransform.sizeDelta = Size;
            rectTransform.pivot = Pivot;
            rectTransform.localRotation = Rotation;
            rectTransform.localScale = Scale;
        }
    }
}

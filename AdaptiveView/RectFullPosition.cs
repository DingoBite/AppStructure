using System;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    [Serializable]
    public class RectFullPosition : FullPosition
    {
        public Vector2 AnchoredPosition;
        public Vector2 Pivot;
        public Vector2 AnchorMax;
        public Vector2 AnchorMin;
        public Vector3 LocalRotation;
        public Vector3 LocalScale = Vector3.one;
        public Vector2 SizeDelta;

        public RectFullPosition(RectTransform rectTransform) : base(rectTransform.gameObject)
        {
            AnchoredPosition = rectTransform.anchoredPosition;
            Pivot = rectTransform.pivot;
            AnchorMin = rectTransform.anchorMin;
            AnchorMax = rectTransform.anchorMax;
            LocalRotation = rectTransform.localRotation.eulerAngles;
            LocalScale = rectTransform.localScale;
            SizeDelta = rectTransform.sizeDelta;
        }

        public RectFullPosition() { }

        protected void Apply(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;
            
            rectTransform.anchoredPosition = AnchoredPosition;
            rectTransform.pivot = Pivot;
            rectTransform.anchorMin = AnchorMin;
            rectTransform.anchorMax = AnchorMax;
            rectTransform.localRotation = Quaternion.Euler(LocalRotation);
            rectTransform.localScale = LocalScale;
            rectTransform.sizeDelta = SizeDelta;
        }

        public override bool Apply<T>(T obj)
        {
            var isType = typeof(T) == typeof(RectTransform);
            if (isType)
            {
                var rectTransform = obj as RectTransform;
                base.Apply(rectTransform?.gameObject);
                Apply(rectTransform);
            }
            return isType;
        }
    }
}
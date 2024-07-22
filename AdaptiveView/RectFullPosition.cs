using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class RectFullPosition : FullPosition
    {
        [SerializeField] public Vector2 AnchoredPosition;
        [SerializeField] public Vector2 Pivot;
        [SerializeField] public Vector2 AnchorMax;
        [SerializeField] public Vector2 AnchorMin;
        [SerializeField] public Vector3 LocalRotation;
        [SerializeField] public Vector3 LocalScale = Vector3.one;
        [SerializeField] public Vector2 SizeDelta;

        public RectFullPosition Initialize(RectTransform rectTransform)
        {
            base.Initialize(rectTransform.gameObject);
            AnchoredPosition = rectTransform.anchoredPosition;
            Pivot = rectTransform.pivot;
            AnchorMin = rectTransform.anchorMin;
            AnchorMax = rectTransform.anchorMax;
            LocalRotation = rectTransform.localRotation.eulerAngles;
            LocalScale = rectTransform.localScale;
            SizeDelta = rectTransform.sizeDelta;
            return this;
        }

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
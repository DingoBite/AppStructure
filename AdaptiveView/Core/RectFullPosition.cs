using UnityEngine;

namespace AppStructure.AdaptiveView.Core
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

        public override void Collect(object obj)
        {
            if (obj is not RectTransform rectTransform)
                return;
            base.Collect(rectTransform.gameObject);
            AnchoredPosition = rectTransform.anchoredPosition;
            Pivot = rectTransform.pivot;
            AnchorMin = rectTransform.anchorMin;
            AnchorMax = rectTransform.anchorMax;
            LocalRotation = rectTransform.localRotation.eulerAngles;
            LocalScale = rectTransform.localScale;
            SizeDelta = rectTransform.sizeDelta;
        }

        public override void Apply(object obj)
        {
            if (obj is not RectTransform rectTransform)
                return;
            
            rectTransform.anchoredPosition = AnchoredPosition;
            rectTransform.pivot = Pivot;
            rectTransform.anchorMin = AnchorMin;
            rectTransform.anchorMax = AnchorMax;
            rectTransform.localRotation = Quaternion.Euler(LocalRotation);
            rectTransform.localScale = LocalScale;
            rectTransform.sizeDelta = SizeDelta;
        }
    }
}
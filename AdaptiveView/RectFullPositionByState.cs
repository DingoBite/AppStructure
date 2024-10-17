using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class RectFullPositionByState : RectFullPosition
    {
        [SerializeField] private RectPositionByState _rectPositionByState;

        public void Initialize(RectPositionByState rectPositionByState)
        {
            base.Initialize(rectPositionByState.RectTransform);
            _rectPositionByState = rectPositionByState;
        }
        
        public override bool Apply<T>(T obj)
        {
            var isType = typeof(T) == typeof(int);
            if (isType)
            {
                var index = (int)(object)obj;
                base.Apply(_rectPositionByState.RectTransform);
                _rectPositionByState.ApplyPosition(index);
            }
            return isType;
        }
    }
}
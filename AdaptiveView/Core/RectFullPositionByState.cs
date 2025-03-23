using System;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class RectFullPositionByState : RectFullPosition
    {
        [SerializeField] private RectPositionByState _rectPositionByState;

        public override void Collect(object obj)
        {
            if (obj is not RectPositionByState rectPositionByState)
                return;
            base.Collect(rectPositionByState.RectTransform);
            _rectPositionByState = rectPositionByState;
        }
        
        public override void Apply(object obj)
        {
            if (obj is not int index)
                return;
            base.Apply(_rectPositionByState.RectTransform);
            _rectPositionByState.ApplyPosition(index);
        }
    }
}
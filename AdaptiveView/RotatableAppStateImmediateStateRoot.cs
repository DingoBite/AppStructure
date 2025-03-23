using System;
using NaughtyAttributes;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public abstract class RotatableAppStateImmediateStateRoot<TState, TAppModel> : ImmediateAppStateRoot<TState, TAppModel>, IScreenOrientationHandler where TState : Enum
    {
        [SerializeField] private ScreenOrientation _screenOrientation;
        [SerializeField] private RectPositionByState _rectPositionByState;

        public void Adapt(ScreenOrientation oldOrientation, ScreenOrientation newOrientation)
        {
            if (_rectPositionByState == null)
                return;
            
            if (newOrientation is ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown)
                _rectPositionByState.ApplyPosition(0);
            else if (newOrientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight)
                _rectPositionByState.ApplyPosition(1);
        }

        [Button]
        private void AdaptDebug() => Adapt(ScreenOrientation.AutoRotation, _screenOrientation);
    }
}
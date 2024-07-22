using System;
using NaughtyAttributes;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public abstract class RotatableAppStateImmediateStateView<TState, TAppModel, TAppConfig> :  ImmediateAppStateView<TState, TAppModel, TAppConfig>, IScreenOrientationHandler where TState : Enum
    {
        [SerializeField] private ScreenOrientation _screenOrientation;
        [SerializeField] private RectPositionByState _rectPositionByState;
        [SerializeField] private ScreenOrientationManage<TState, TAppModel, TAppConfig> _screenOrientationManage;

        protected override void StartEnable()
        {
            Adapt(ScreenOrientation.AutoRotation, _screenOrientationManage.CurrentOrientation);
            base.StartEnable();
        }

        public void Adapt(ScreenOrientation oldOrientation, ScreenOrientation newOrientation)
        {
            if (_rectPositionByState == null)
                return;
            
            if (newOrientation is ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown)
                _rectPositionByState.ApplyPosition(0);
            else if (newOrientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight)
                _rectPositionByState.ApplyPosition(1);
        }

        private void Reset()
        {
            _screenOrientationManage = FindFirstObjectByType<ScreenOrientationManage<TState, TAppModel, TAppConfig>>(FindObjectsInactive.Include);
        }

        [Button]
        private void AdaptDebug() => Adapt(ScreenOrientation.AutoRotation, _screenOrientation);
    }
}
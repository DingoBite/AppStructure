using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using DingoUnityExtensions;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public abstract class ScreenOrientationManage<TState, TAppModel, TAppConfig> : StaticStateViewElement<TState, TAppModel, TAppConfig> where TState : Enum
    {
        public event Action<ScreenOrientation, ScreenOrientation> OrientationChange;

        [SerializeField] private List<RotatableAppStateImmediateStateView<TState, TAppModel, TAppConfig>> _screenOrientationHandlers;
        [SerializeField] private List<TState> _onlyPortraitScreens;

        private ScreenOrientation _previousOrientation = ScreenOrientation.Portrait;
        public ScreenOrientation CurrentOrientation => _previousOrientation;
        
        public override Task InitializeAsync(TAppConfig appConfig)
        {
            CoroutineParent.AddUpdater(this, OnUpdate);
            return base.InitializeAsync(appConfig);
        }

        public override void Transfer(TransferInfo<TState> transferInfo)
        {
            if (!_onlyPortraitScreens.Contains(transferInfo.To))
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                return;
            }

            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void OnUpdate()
        {
            var orientation = Screen.orientation;
            if (orientation == _previousOrientation)
                return;
            foreach (var screenOrientationHandler in _screenOrientationHandlers.Where(h => h != null))
            {
                screenOrientationHandler.Adapt(_previousOrientation, orientation);
            }
            OrientationChange?.Invoke(_previousOrientation, orientation);
            if (Screen.autorotateToPortrait)
                _previousOrientation = orientation;
        }
    }
}
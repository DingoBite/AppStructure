using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using DingoUnityExtensions;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public abstract class ScreenOrientationManage<TState, TAppModel> : StaticStateViewElement<TState, TAppModel> where TState : Enum
    {
        [SerializeField] private List<RotatableAppStateImmediateStateRoot<TState, TAppModel>> _screenOrientationHandlers;
        [SerializeField] private List<TState> _onlyPortraitScreens;
        [SerializeField] private List<TState> _onlyLandscapeScreens;

        private ScreenOrientation _currentOrientation = ScreenOrientation.Portrait;
        
        public override Task InitializeAsync()
        {
            CoroutineParent.AddUpdater(this, OnUpdate);
            return base.InitializeAsync();
        }

        public override void Transfer(TransferInfo<TState> transferInfo)
        {
            if (_onlyPortraitScreens.Contains(transferInfo.To))
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }
            else if (_onlyLandscapeScreens.Contains(transferInfo.To))
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
            else
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                foreach (var handler in _screenOrientationHandlers)
                {
                    handler.Adapt(ScreenOrientation.AutoRotation, _currentOrientation);
                }
            }
        }

        private void OnUpdate()
        {
            var orientation = Screen.orientation;
            if (orientation == _currentOrientation)
                return;
            foreach (var screenOrientationHandler in _screenOrientationHandlers.Where(h => h != null))
            {
                screenOrientationHandler.Adapt(_currentOrientation, orientation);
            }
            if (Screen.autorotateToPortrait)
                _currentOrientation = orientation;
        }
    }
}
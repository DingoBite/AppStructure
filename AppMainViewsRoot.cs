using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace AppStructure
{
    public abstract class AppMainViewsRoot<TState, TAppModel, TAppConfig> : AppPartRoot<TAppModel, TAppConfig> where TState : Enum
    {
        [SerializedDictionary("State", "View")]
        [SerializeField] private SerializedDictionary<TState, AppStateView<TState, TAppModel, TAppConfig>> _stateViews;
        [SerializeField] private List<StaticStateViewElement<TState, TAppModel, TAppConfig>> _staticScreenViewElements;

        public override void PreInitialize()
        {
            _stateViews.Values.ProcessAppStateViews(s => s.PreInitialize());
            _staticScreenViewElements.ProcessStaticStateViews(s => s.PreInitialize());
        }

        public override async Task<bool> InitializeAsync(TAppConfig appConfig)
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(s => s.InitializeAsync(appConfig));
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(s => s.InitializeAsync(appConfig));
            return true;
        }

        public override async Task<bool> BindAsync(TAppModel appModel) 
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(s => s.BindAsync(appModel));
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(s => s.BindAsync(appModel));
            return true;
        }

        public override async Task<bool> PostInitializeAsync()
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(g => g.PostInitializeAsync());
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(g => g.PostInitializeAsync());
            return true;
        }
        
        public bool ApplyTransfer(TransferInfo<TState> transferInfo)
        {
            if (transferInfo.IsNone) return false;

            try
            {
                if (_stateViews.TryGetValue(transferInfo.From, out var stateView))
                    FromStateViewTransferHandle(transferInfo, stateView);

                if (_stateViews.TryGetValue(transferInfo.To, out stateView))
                    ToStateViewTransferHandle(transferInfo, stateView);
                
                foreach (var element in _staticScreenViewElements)
                {
                    element.Transfer(transferInfo);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot apply transfer {transferInfo}");
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        protected virtual void ToStateViewTransferHandle(TransferInfo<TState> transferInfo, AppStateView<TState, TAppModel, TAppConfig> appStateView) => appStateView.EnableOnTransfer(transferInfo);
        protected virtual void FromStateViewTransferHandle(TransferInfo<TState> transferInfo, AppStateView<TState, TAppModel, TAppConfig> appStateView) => appStateView.DisableOnTransfer(transferInfo);
    }
}
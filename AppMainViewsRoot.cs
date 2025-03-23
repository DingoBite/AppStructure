using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using AppStructure.Utils;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace AppStructure
{
    public abstract class AppMainViewsRoot<TState, TAppModel> : AppPartRoot<TAppModel>
    {
        private const string STATE_PREFIX = "-s-";

        [SerializedDictionary("State", "View")]
        [SerializeField] private SerializedDictionary<TState, AppStateRoot<TState, TAppModel>> _stateViews;
        [SerializeField] private List<StaticStateViewElement<TState, TAppModel>> _staticScreenViewElements;
        [SerializeField] private List<StaticViewElement<TAppModel>> _generalViewElements;

        public IEnumerable<TState> States => _stateViews.Keys;
        
        public override void PreInitialize()
        {
            _stateViews.Values.ProcessAppStateViews(s => s.PreInitialize());
            _staticScreenViewElements.ProcessStaticStateViews(s => s.PreInitialize());
        }

        public override async Task<bool> InitializeAsync()
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(s => s.InitializeAsync());
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(s => s.InitializeAsync());
            await _generalViewElements.ProcessGeneralViewElementsAsync(s => s.InitializeAsync());
            return true;
        }

        public override async Task<bool> BindAsync(TAppModel appModel) 
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(s => s.BindAsync(appModel));
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(s => s.BindAsync(appModel));
            await _generalViewElements.ProcessGeneralViewElementsAsync(s => s.BindAsync(appModel));
            return true;
        }

        public override async Task<bool> PostInitializeAsync()
        {
            await _stateViews.Values.ProcessAppStateViewsAsync(g => g.PostInitializeAsync());
            await _staticScreenViewElements.ProcessStaticStateViewsAsync(g => g.PostInitializeAsync());
            await _generalViewElements.ProcessGeneralViewElementsAsync(s => s.PostInitializeAsync());
            return true;
        }
        
        public async Task<bool> ApplyTransferAsync(TransferInfo<TState> transferInfo)
        {
            if (transferInfo.IsNone) return false;

            try
            {
                if (_stateViews.TryGetValue(transferInfo.From, out var stateView))
                    await FromStateViewTransferHandleAsync(transferInfo, stateView);

                if (_stateViews.TryGetValue(transferInfo.To, out stateView))
                    await ToStateViewTransferHandleAsync(transferInfo, stateView);
                
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

        private void OnValidate()
        {
            foreach (var (key, stateView) in _stateViews)
            {
                stateView.name = $"{STATE_PREFIX}{key}";
            }
        }

        private static async Task ToStateViewTransferHandleAsync(TransferInfo<TState> transferInfo, AppStateRoot<TState, TAppModel> appStateRoot) => await appStateRoot.EnableOnTransferAsync(transferInfo);
        private static async Task FromStateViewTransferHandleAsync(TransferInfo<TState> transferInfo, AppStateRoot<TState, TAppModel> appStateRoot) => await appStateRoot.DisableOnTransferAsync(transferInfo);
    }
}
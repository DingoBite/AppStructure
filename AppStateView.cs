using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using UnityEngine;

namespace AppStructure
{
    public abstract class AppStateView<TState, TAppModel, TAppConfig> : AppPartRoot<TAppModel, TAppConfig> where TState : Enum
    {
        [SerializeField] protected List<GeneralViewElement<TAppModel, TAppConfig>> GeneralElements;
        [SerializeField] protected List<StateViewElement<TState, TAppModel, TAppConfig>> StateElements;
        
        private readonly List<Action> _cachedEnableChanges = new ();

        public bool IsActive { get; protected set; }

        public override void PreInitialize()
        {
            GeneralElements.ProcessGeneralViewElements(s => s.PreInitialize());
            StateElements.ProcessStateViewElements(s => s.PreInitialize());
            SetDefaultValues();
        }
        
        public override async Task<bool> InitializeAsync(TAppConfig appConfig)
        {
            await GeneralElements.ProcessGeneralViewElementsAsync(s => s.InitializeAsync(appConfig));
            await StateElements.ProcessStateViewElementsAsync(s => s.InitializeAsync(appConfig));
            return true;
        }

        public override async Task<bool> BindAsync(TAppModel appModel) 
        {
            await GeneralElements.ProcessGeneralViewElementsAsync(s => s.BindAsync(appModel));
            await StateElements.ProcessStateViewElementsAsync(s => s.BindAsync(appModel));
            return true;
        }

        public override async Task<bool> PostInitializeAsync()
        {
            await GeneralElements.ProcessGeneralViewElementsAsync(g => g.PostInitializeAsync());
            await StateElements.ProcessStateViewElementsAsync(g => g.PostInitializeAsync());
            return true;
        }

        public virtual async Task EnableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            await StateElements.ProcessStateViewElementsAsync(s => s.EnableElementAsync(transferInfo));
        }

        public virtual async Task DisableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            IsActive = false;
            await StateElements.ProcessStateViewElementsAsync(s => s.DisableElementAsync(transferInfo));
        }

        protected void InvokeOrWaitActiveCacheAction(Action action)
        {
            if (IsActive)
                action?.Invoke();
            else if (!_cachedEnableChanges.Contains(action))
                _cachedEnableChanges.Add(action);
        }
        
        protected virtual void StartEnable()
        {
            gameObject.SetActive(true);
            IsActive = true;
            foreach (var action in _cachedEnableChanges)
            {
                action?.Invoke();
            }
            _cachedEnableChanges.Clear();
            
            StateElements.ProcessStateViewElements(s => s.OnStartScreenEnable());
        }

        protected virtual void DisableCompletely()
        {
            SetDefaultValues();
            if (StateElements == null) return;
            
            StateElements.ProcessStateViewElements(s => s.OnCompletelyDisable());
        }

        protected abstract void SetDefaultValues();
    }
}
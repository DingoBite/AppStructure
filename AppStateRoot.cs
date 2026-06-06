using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using AppStructure.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AppStructure
{
    public abstract class AppStateRoot<TState, TAppModel> : AppPartRoot<TAppModel>
    {
        [SerializeField] protected List<StateViewElement<TState, TAppModel>> _stateElements;
        [SerializeField] protected List<StaticStateViewElement<TState, TAppModel>> _staticElements;

        public bool IsActive { get; protected set; }

        public override void PreInitialize()
        {
            _staticElements.ProcessStaticViewElements(s => s.PreInitialize());
            _stateElements.ProcessStateViewElements(s => s.PreInitialize());
            SetDefaultValues();
        }
        
        public override async Task<bool> InitializeAsync()
        {
            await _staticElements.ProcessStaticViewElementsAsync(s => s.InitializeAsync());
            await _stateElements.ProcessStateViewElements(s => s.InitializeAsync());
            return true;
        }

        public override async Task<bool> BindAsync(TAppModel appModel) 
        {
            await _staticElements.ProcessStaticViewElementsAsync(s => s.BindAsync(appModel));
            await _stateElements.ProcessStateViewElements(s => s.BindAsync(appModel));
            return true;
        }

        public override async Task<bool> PostInitializeAsync()
        {
            await _staticElements.ProcessStaticViewElementsAsync(g => g.PostInitializeAsync());
            await _stateElements.ProcessStateViewElements(g => g.PostInitializeAsync());
            return true;
        }

        public virtual void EnableOnTransfer(TransferInfo<TState> transferInfo)
        {
            _staticElements.ProcessStaticStateViews(s => s.Enable(transferInfo));
            _stateElements.ProcessStateViewElements(s => s.EnableElement(transferInfo));
        }

        public virtual void DisableOnTransfer(TransferInfo<TState> transferInfo)
        {
            _staticElements.ProcessStaticStateViews(s => s.Disable(transferInfo));
            _stateElements.ProcessStateViewElements(s => s.DisableElement(transferInfo));
        }

        protected virtual void StartDisable(TransferInfo<TState> transferInfo)
        {
            IsActive = false;
            _stateElements.ProcessStateViewElements(s => s.OnStartScreenDisable(transferInfo));
        }
        
        protected virtual void StartEnable(TransferInfo<TState> transferInfo)
        {
            gameObject.SetActive(true);
            IsActive = true;

            _stateElements.ProcessStateViewElements(s => s.OnStartStateEnable(transferInfo));
        }

        protected virtual void DisableCompletely(TransferInfo<TState> transferInfo)
        {
            SetDefaultValues();
            if (_stateElements == null) 
                return;
            
            _stateElements.ProcessStateViewElements(s => s.OnCompletelyDisable(transferInfo));
        }

        protected abstract void SetDefaultValues();
        
        protected IEnumerable<MonoBehaviour> GetAllElementsBehaviours()
        {
            return _staticElements.Select(e => (MonoBehaviour) e).Concat(_stateElements.Select(e => (MonoBehaviour) e));
        }
    }
}
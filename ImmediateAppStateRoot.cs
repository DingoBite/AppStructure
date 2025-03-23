using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure
{
    public abstract class ImmediateAppStateRoot<TState, TAppModel> : AppStateRoot<TState, TAppModel>
    {
        private Canvas _canvas;

        protected Canvas Canvas => _canvas ??= GetComponent<Canvas>();
        
        protected override void StartEnable(TransferInfo<TState> transferInfo)
        {
            if (Canvas != null)
                Canvas.enabled = true;
            
            base.StartEnable(transferInfo);
        }
        
        protected override void DisableCompletely(TransferInfo<TState> transferInfo)
        {
            base.DisableCompletely(transferInfo);
            if (Canvas != null)
                Canvas.enabled = false;
        }

        public override async Task DisableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            StartDisable(transferInfo);
            await base.DisableOnTransferAsync(transferInfo);
            DisableCompletely(transferInfo);
        }

        public override async Task EnableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            StartEnable(transferInfo);
            await base.EnableOnTransferAsync(transferInfo);
        }

        protected sealed override void SetDefaultValues()
        {
            IsActive = false;
            
            if (Canvas != null)
                Canvas.enabled = false;

            gameObject.SetActive(false);
        }
    }
}
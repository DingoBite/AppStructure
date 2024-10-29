using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure
{
    public abstract class ImmediateAppStateView<TState, TAppModel, TAppConfig> : AppStateView<TState, TAppModel, TAppConfig> where TState : Enum
    {
        private Canvas _canvas;

        protected Canvas Canvas => _canvas ??= GetComponent<Canvas>();
        
        protected override void StartEnable()
        {
            if (Canvas != null)
                Canvas.enabled = true;
            
            base.StartEnable();
        }

        protected override void DisableCompletely()
        {
            base.DisableCompletely();
            if (Canvas != null)
                Canvas.enabled = false;
        }

        public override async Task DisableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            await base.DisableOnTransferAsync(transferInfo);
            DisableCompletely();
        }

        public override async Task EnableOnTransferAsync(TransferInfo<TState> transferInfo)
        {
            StartEnable();
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
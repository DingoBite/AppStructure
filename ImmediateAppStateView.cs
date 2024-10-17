using System;
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

        public override void DisableOnTransfer(TransferInfo<TState> transferInfo)
        {
            base.DisableOnTransfer(transferInfo);
            DisableCompletely();
        }

        protected override void DisableCompletely()
        {
            base.DisableCompletely();
            if (Canvas != null)
                Canvas.enabled = false;
        }

        public override void EnableOnTransfer(TransferInfo<TState> transferInfo)
        {
            StartEnable();
            base.EnableOnTransfer(transferInfo);
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
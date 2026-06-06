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

        public override void DisableOnTransfer(TransferInfo<TState> transferInfo)
        {
            StartDisable(transferInfo);
            DisableCompletely(transferInfo);
        }

        public override void EnableOnTransfer(TransferInfo<TState> transferInfo) => StartEnable(transferInfo);

        protected sealed override void SetDefaultValues()
        {
            IsActive = false;
            
            if (Canvas != null)
                Canvas.enabled = false;

            gameObject.SetActive(false);
        }
    }
}
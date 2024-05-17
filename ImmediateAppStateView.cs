using System;

namespace AppStructure
{
    public abstract class ImmediateAppStateView<TState, TAppModel, TAppConfig> : AppStateView<TState, TAppModel, TAppConfig> where TState : Enum
    {
        public override void DisableOnTransfer(TransferInfo<TState> transferInfo)
        {
            base.DisableOnTransfer(transferInfo);
            DisableCompletely();
        }

        public override void EnableOnTransfer(TransferInfo<TState> transferInfo)
        {
            StartEnable();
            base.EnableOnTransfer(transferInfo);
        }

        protected sealed override void SetDefaultValues()
        {
            IsActive = false;
            
            gameObject.SetActive(false);
        }
    }
}
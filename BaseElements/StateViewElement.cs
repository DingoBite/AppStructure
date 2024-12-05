using System;
using System.Threading.Tasks;
using AppStructure.Utils;

namespace AppStructure.BaseElements
{
    public abstract class StateViewElement<TState, TAppModel, TAppConfig> : SubscribableBehaviour where TState : Enum
    {
        private bool _binded;
        protected sealed override bool ForSubscriptionPrepared => _binded;

        public virtual void SetDefaultValues() {}
        
        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync(TAppConfig appConfig) => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel)
        {
            _binded = true;
            if (isActiveAndEnabled)
            {
                UnsubscribeOnly();
                SubscribeOnly();
            }
            return Task.CompletedTask;
        }

        public virtual Task PostInitializeAsync() => Task.CompletedTask;

        public abstract Task EnableElementAsync(TransferInfo<TState> transferInfo);
        public abstract void OnStartScreenEnable();

        public abstract Task DisableElementAsync(TransferInfo<TState> transferInfo);
        public abstract void OnCompletelyDisable();
        
        protected override void SubscribeOnly() {}
        protected override void UnsubscribeOnly() {}
    }
}
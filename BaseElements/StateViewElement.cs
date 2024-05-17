using System;
using System.Threading.Tasks;
using AppStructure.Utils;

namespace AppStructure.BaseElements
{
    public abstract class StateViewElement<TState, TAppModel, TAppConfig> : SubscribableBehaviour where TState : Enum
    {
        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync(TAppConfig appConfig) => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel) => Task.CompletedTask;
        public virtual Task PostInitializeAsync() => Task.CompletedTask;

        public abstract void EnableElement(TransferInfo<TState> transferInfo);
        public abstract void OnStartScreenEnable();

        public abstract void DisableElement(TransferInfo<TState> transferInfo);
        public abstract void OnCompletelyDisable();
        
        protected override void SubscribeOnly() {}
        protected override void UnsubscribeOnly() {}
    }
}
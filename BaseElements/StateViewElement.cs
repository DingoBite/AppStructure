using System;
using System.Threading.Tasks;
using AppStructure.Utils;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public abstract class StateViewElement<TState, TAppModel, TAppConfig> : SubscribableBehaviour where TState : Enum
    {
        [SerializeField] private bool _enableDisableFromState = true;
        
        private bool _binded;
        protected sealed override bool ForSubscriptionPrepared => !_enableDisableFromState && _binded;

        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync(TAppConfig appConfig) => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel)
        {
            _binded = true;
            if (ForSubscriptionPrepared && isActiveAndEnabled)
            {
                UnsubscribeOnly();
                SubscribeOnly();
            }
            return Task.CompletedTask;
        }

        public virtual Task PostInitializeAsync() => Task.CompletedTask;

        public abstract Task EnableElementAsync(TransferInfo<TState> transferInfo);

        public virtual void OnStartScreenEnable()
        {
            if (_enableDisableFromState)
            {
                UnsubscribeOnly();
                SubscribeOnly();
            }
        }

        public virtual Task DisableElementAsync(TransferInfo<TState> transferInfo)
        {
            if (_enableDisableFromState)
                UnsubscribeOnly();
            return Task.CompletedTask;
        }
        
        public abstract void OnCompletelyDisable();
        
        protected override void SubscribeOnly() {}
        protected override void UnsubscribeOnly() {}
    }
}
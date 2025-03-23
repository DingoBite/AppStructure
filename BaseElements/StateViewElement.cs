using System;
using System.Threading.Tasks;
using AppStructure.Utils;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public abstract class StateViewElement<TState, TAppModel> : MonoBehaviour
    {
        private bool _enabled;
        
        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel)
        {
            if (_enabled)
            {
                UnsubscribeOnly();
                SubscribeOnly();
            }
            return Task.CompletedTask;
        }

        public virtual Task PostInitializeAsync() => Task.CompletedTask;
        
        public virtual Task EnableElementAsync(TransferInfo<TState> transferInfo) => Task.CompletedTask;

        public virtual void OnStartStateEnable(TransferInfo<TState> transferInfo)
        {
            _enabled = true;
            UnsubscribeOnly();
            SubscribeOnly();
        }

        public virtual void OnStartScreenDisable(TransferInfo<TState> transferInfo)
        {
            _enabled = false;
            UnsubscribeOnly();
        }
        
        public virtual Task DisableElementAsync(TransferInfo<TState> transferInfo) => Task.CompletedTask;
        public virtual void OnCompletelyDisable(TransferInfo<TState> transferInfo) { }
        
        protected virtual void SubscribeOnly() {}
        protected virtual void UnsubscribeOnly() {}
    }
}
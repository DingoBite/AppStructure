using System;
using System.Threading.Tasks;

namespace AppStructure.BaseElements
{
    public abstract class StaticStateViewElement<TState, TAppModel> : StaticViewElement<TAppModel>
    {
        protected bool Enabled { get; private set; }

        public override Task BindAsync(TAppModel appModel)
        {
            if (Enabled)
            {
                UnsubscribeOnly();
                SubscribeOnly();
            }
            return base.BindAsync(appModel);
        }

        public virtual void Enable(TransferInfo<TState> transferInfo)
        {
            Enabled = true;
            UnsubscribeOnly();
            SubscribeOnly();
        }
        
        public virtual void StaticTransfer(TransferInfo<TState> transferInfo) { }

        public virtual void Disable(TransferInfo<TState> transferInfo)
        {
            Enabled = false;
            UnsubscribeOnly();
        }
        
        protected virtual void SubscribeOnly() {}
        protected virtual void UnsubscribeOnly() {}
    }
}
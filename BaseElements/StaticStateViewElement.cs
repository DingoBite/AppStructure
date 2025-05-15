using System;

namespace AppStructure.BaseElements
{
    public abstract class StaticStateViewElement<TState, TAppModel> : StaticViewElement<TAppModel>
    {
        public virtual void Enable(TransferInfo<TState> transferInfo) { }
        public virtual void StaticTransfer(TransferInfo<TState> transferInfo) { }
        public virtual void Disable(TransferInfo<TState> transferInfo) { }
    }
}
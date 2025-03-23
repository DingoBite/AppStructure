using System;

namespace AppStructure.BaseElements
{
    public abstract class StaticStateViewElement<TState, TAppModel> : StaticViewElement<TAppModel>
    {
        public virtual void Transfer(TransferInfo<TState> transferInfo) { }
    }
}
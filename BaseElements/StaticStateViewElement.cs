using System;

namespace AppStructure.BaseElements
{
    public abstract class StaticStateViewElement<TState, TAppModel, TAppConfig> : GeneralViewElement<TAppModel, TAppConfig> where TState : Enum
    {
        public abstract void Transfer(TransferInfo<TState> transferInfo);
    }
}
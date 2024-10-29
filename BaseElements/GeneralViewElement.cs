using System.Threading.Tasks;
using AppStructure.Utils;

namespace AppStructure.BaseElements
{
    public abstract class GeneralViewElement<TAppModel, TAppConfig> : SubscribableBehaviour
    {
        public virtual void SetDefaultValues() {}
        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync(TAppConfig appConfig) => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel) => Task.CompletedTask;
        public virtual Task PostInitializeAsync() => Task.CompletedTask;
        protected override void SubscribeOnly() {}
        protected override void UnsubscribeOnly() {}
    }
}
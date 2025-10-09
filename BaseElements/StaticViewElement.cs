using System.Threading.Tasks;
using AppStructure.Utils;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public abstract class StaticViewElement<TAppModel> : MonoBehaviour
    {
        public virtual void PreInitialize() {}
        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual Task BindAsync(TAppModel appModel) => Task.CompletedTask;
        public virtual Task PostInitializeAsync() => Task.CompletedTask;
    }
}
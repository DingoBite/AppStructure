using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public abstract class AppPartRoot<TAppModel, TAppConfig> : MonoBehaviour
    {
        public abstract void PreInitialize();
        public abstract Task<bool> InitializeAsync(TAppConfig appConfig);
        public abstract Task<bool> BindAsync(TAppModel appModel);
        public abstract Task<bool> PostInitializeAsync();
    }
}
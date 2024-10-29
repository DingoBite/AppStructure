using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public interface IAppStructurePart<in TAppModel, in TAppConfig>
    {
        public void PreInitialize();
        public Task<bool> InitializeAsync(TAppConfig appConfig);
        public Task<bool> BindAsync(TAppModel appModel);
        public Task<bool> PostInitializeAsync();
    }

    public abstract class AppPartRoot<TAppModel, TAppConfig> : MonoBehaviour, IAppStructurePart<TAppModel, TAppConfig>
    {
        public abstract void PreInitialize();
        public abstract Task<bool> InitializeAsync(TAppConfig appConfig);
        public abstract Task<bool> BindAsync(TAppModel appModel);
        public abstract Task<bool> PostInitializeAsync();
    }
}
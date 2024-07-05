using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public interface IAppStructurePart<in TAppModel, in TAppConfig>
    {
        void PreInitialize();
        Task<bool> InitializeAsync(TAppConfig appConfig);
        Task<bool> BindAsync(TAppModel appModel);
        Task<bool> PostInitializeAsync();
    }

    public abstract class AppPartRoot<TAppModel, TAppConfig> : MonoBehaviour, IAppStructurePart<TAppModel, TAppConfig>
    {
        public abstract void PreInitialize();
        public abstract Task<bool> InitializeAsync(TAppConfig appConfig);
        public abstract Task<bool> BindAsync(TAppModel appModel);
        public abstract Task<bool> PostInitializeAsync();
    }
}
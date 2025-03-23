using System.Threading.Tasks;
using UnityEngine;

namespace AppStructure.BaseElements
{
    public interface IAppStructurePart<in TAppModel>
    {
        public void PreInitialize();
        public Task<bool> InitializeAsync();
        public Task<bool> BindAsync(TAppModel appModel);
        public Task<bool> PostInitializeAsync();
    }

    public abstract class AppPartRoot<TAppModel> : MonoBehaviour, IAppStructurePart<TAppModel>
    {
        public abstract void PreInitialize();
        public abstract Task<bool> InitializeAsync();
        public abstract Task<bool> BindAsync(TAppModel appModel);
        public abstract Task<bool> PostInitializeAsync();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AppStructure.Utils
{
    public static class Extensions
    {
        public static void ProcessCollectionErrorHandle<T>(this IEnumerable<T> collection, Action<T> processAction, Func<T, string> catchMessage) where T : Object
        {
            if (collection == null)
                return;
            
            foreach (var element in collection.Where(s => s != null))
            {
                try
                {
                    processAction.Invoke(element);
                }
                catch (Exception e)
                {
                    Debug.LogError(catchMessage(element));
                    Debug.LogException(e, element);
                }
            }
        }
        
        public static async Task ProcessCollectionErrorHandleAsync<T>(this IEnumerable<T> collection, Func<T, Task> processAction, Func<T, string> catchMessage) where T : Object
        {
            if (collection == null)
                return;
            
            foreach (var element in collection.Where(s => s != null))
            {
                try
                {
                    await processAction.Invoke(element);
                }
                catch (Exception e)
                {
                    Debug.LogError(catchMessage(element));
                    Debug.LogException(e, element);
                }
            }
        }

        public static void ProcessAppStateViews<TState, TAppModel>(this IEnumerable<AppStateRoot<TState, TAppModel>> appStateViews,
            Action<AppStateRoot<TState, TAppModel>> processAction)
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for AppStateView");
        }
        
        public static async Task ProcessAppStateViewsAsync<TState, TAppModel>(this IEnumerable<AppStateRoot<TState, TAppModel>> appStateViews,
            Func<AppStateRoot<TState, TAppModel>, Task> processAction)
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for AppStateView");
        }
        
        public static void ProcessStateViewElements<TState, TAppModel>(this IEnumerable<StateViewElement<TState, TAppModel>> appStateViews,
            Action<StateViewElement<TState, TAppModel>> processAction)
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for StateViewElement");
        }
        
        public static async Task ProcessStateViewElementsAsync<TState, TAppModel>(this IEnumerable<StateViewElement<TState, TAppModel>> appStateViews,
            Func<StateViewElement<TState, TAppModel>, Task> processAction)
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for StateViewElement");
        }
        
        public static void ProcessStaticStateViews<TState, TAppModel>(this IEnumerable<StaticStateViewElement<TState, TAppModel>> appStateViews,
            Action<StaticStateViewElement<TState, TAppModel>> processAction)
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for StaticStateViewElement");
        }
        
        public static async Task ProcessStaticStateViewsAsync<TState, TAppModel>(this IEnumerable<StaticStateViewElement<TState, TAppModel>> appStateViews,
            Func<StaticStateViewElement<TState, TAppModel>, Task> processAction)
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for StaticStateViewElement");
        }
        
        public static void ProcessGeneralViewElements<TAppModel>(this IEnumerable<StaticViewElement<TAppModel>> appStateViews,
            Action<StaticViewElement<TAppModel>> processAction)
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for GeneralView");
        }
        
        public static async Task ProcessGeneralViewElementsAsync<TAppModel>(this IEnumerable<StaticViewElement<TAppModel>> appStateViews,
            Func<StaticViewElement<TAppModel>, Task> processAction)
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for GeneralView");
        }
    }
}
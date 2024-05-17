using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppStructure.BaseElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AppStructure
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

        public static void ProcessAppStateViews<TState, TAppModel, TAppConfig>(this IEnumerable<AppStateView<TState, TAppModel, TAppConfig>> appStateViews,
            Action<AppStateView<TState, TAppModel, TAppConfig>> processAction) where TState : Enum
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for AppStateView");
        }
        
        public static async Task ProcessAppStateViewsAsync<TState, TAppModel, TAppConfig>(this IEnumerable<AppStateView<TState, TAppModel, TAppConfig>> appStateViews,
            Func<AppStateView<TState, TAppModel, TAppConfig>, Task> processAction) where TState : Enum
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for AppStateView");
        }
        
        public static void ProcessStateViewElements<TState, TAppModel, TAppConfig>(this IEnumerable<StateViewElement<TState, TAppModel, TAppConfig>> appStateViews,
            Action<StateViewElement<TState, TAppModel, TAppConfig>> processAction) where TState : Enum
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for StateViewElement");
        }
        
        public static async Task ProcessStateViewElementsAsync<TState, TAppModel, TAppConfig>(this IEnumerable<StateViewElement<TState, TAppModel, TAppConfig>> appStateViews,
            Func<StateViewElement<TState, TAppModel, TAppConfig>, Task> processAction) where TState : Enum
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for StateViewElement");
        }
        
        public static void ProcessStaticStateViews<TState, TAppModel, TAppConfig>(this IEnumerable<StaticStateViewElement<TState, TAppModel, TAppConfig>> appStateViews,
            Action<StaticStateViewElement<TState, TAppModel, TAppConfig>> processAction) where TState : Enum
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for StaticStateViewElement");
        }
        
        public static async Task ProcessStaticStateViewsAsync<TState, TAppModel, TAppConfig>(this IEnumerable<StaticStateViewElement<TState, TAppModel, TAppConfig>> appStateViews,
            Func<StaticStateViewElement<TState, TAppModel, TAppConfig>, Task> processAction) where TState : Enum
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for StaticStateViewElement");
        }
        
        public static void ProcessGeneralViewElements<TAppModel, TAppConfig>(this IEnumerable<GeneralViewElement<TAppModel, TAppConfig>> appStateViews,
            Action<GeneralViewElement<TAppModel, TAppConfig>> processAction)
        {
            appStateViews.ProcessCollectionErrorHandle(processAction, e => $"Cannot apply {nameof(processAction)} for GeneralView");
        }
        
        public static async Task ProcessGeneralViewElementsAsync<TAppModel, TAppConfig>(this IEnumerable<GeneralViewElement<TAppModel, TAppConfig>> appStateViews,
            Func<GeneralViewElement<TAppModel, TAppConfig>, Task> processAction)
        {
            await appStateViews.ProcessCollectionErrorHandleAsync(processAction, e => $"Cannot async apply {nameof(processAction)} for GeneralView");
        }
    }
}
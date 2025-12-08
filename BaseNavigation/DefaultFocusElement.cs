using System.Collections.Generic;
using System.Linq;
using DingoProjectAppStructure.Core.AppRootCore;
using DingoUnityExtensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AppStructure.BaseNavigation
{
    public interface IFocusProvider
    {
        public int Layer { get; }
        public GameObject FocusNode { get; }
    }

    public static class FocusManager
    {
        private static readonly List<IFocusProvider> Providers = new();
        private static readonly Dictionary<IFocusProvider, GameObject> LastSelected = new();

        private static IFocusProvider _best;

        public static void Register(IFocusProvider provider)
        {
            if (!Providers.Contains(provider))
                Providers.Add(provider);

            CoroutineParent.AddUpdater(Providers, UpdateFocus, CoroutineOrderLayers.MIN_PRIORITY_SPECIAL);
        }

        public static void Unregister(IFocusProvider provider)
        {
            Providers.Remove(provider);
            LastSelected.Remove(provider);

            if (_best == provider)
                _best = null;

            if (Providers.Count == 0)
                CoroutineParent.RemoveUpdater(Providers);
        }

        public static void UpdateFocus()
        {
            if (EventSystem.current == null)
                return;

            var es = EventSystem.current;
            var current = es.currentSelectedGameObject;

            if (_best != null && current != null && current.activeInHierarchy)
                LastSelected[_best] = current;

            var prev = _best;
            _best = Providers.Where(p => p != null && p.FocusNode != null).OrderByDescending(p => p.Layer).FirstOrDefault();

            if (_best == null)
                return;

            var providerChanged = prev != _best;
            GameObject target = null;

            if (providerChanged || current == null || !current.activeInHierarchy)
            {
                if (!LastSelected.Remove(_best, out var last) || last == null || !last.activeInHierarchy)
                    target = _best.FocusNode;
                else
                    target = last;
            }

            if (target != null && current != target)
                es.SetSelectedGameObject(target);
        }

        public static void ForceUpdate() => UpdateFocus();
    }

    public class DefaultFocusElement : AppStateElementBehaviour, IFocusProvider
    {
        [SerializeField] private GameObject _navigationNode;
        [SerializeField] private int _layerForSelecting;

        public int Layer => _layerForSelecting;
        public GameObject FocusNode => _navigationNode;

        public override void OnStartStateEnable(TransferInfo<string> transferInfo)
        {
            FocusManager.Register(this);
            base.OnStartStateEnable(transferInfo);
        }

        public override void OnStartScreenDisable(TransferInfo<string> transferInfo)
        {
            FocusManager.Unregister(this);
            base.OnStartScreenDisable(transferInfo);
        }

        public void RewriteFocusElement(GameObject navigationNode)
        {
            _navigationNode = navigationNode;
        }
    }
}
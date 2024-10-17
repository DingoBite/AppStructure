using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.MonoBehaviours.UI;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class RectPositionByState : UIBehaviour
    {
        [SerializeField, Dropdown(nameof(States)), Tooltip("If index == -1, then bake new state.")]
        private int _indexForBake = -1;
        
        [SerializeField] private SerializedDictionary<RectTransform, FullPosList> _rectPositions = new();
        [SerializeField] private List<AdaptByStateElement> _adaptByStateElements = new();
        
        private List<int> States => Enumerable.Empty<int>().Append(-1).Concat(Enumerable.Range(0, MaxCount)).ToList();
        private int MaxCount
        {
            get
            {
                var firstOrDefault = _rectPositions.Where(p => p.Value != null).OrderBy(e => e.Value.Count).FirstOrDefault();
                if (firstOrDefault.Value == default)
                    return 0;
                return firstOrDefault.Value.Count;
            }
        }

        public void ApplyPosition(int index)
        {
            foreach (var (rt, values) in _rectPositions)
            {
                if (rt == null || values == null || values.Count == 0)
                    return;
                var i = Math.Min(index, values.Count - 1);
                var fullPos = values.RectFullPositions[i];

                if (rt.TryGetComponent<TMP_Text>(out var text))
                    fullPos.Apply(text);
                else
                    fullPos.Apply(rt);
            }

            foreach (var adaptByStateElement in _adaptByStateElements)
            {
                adaptByStateElement.Adapt(index);
            }
        }
        
        private void ChangeBakedState()
        {
            foreach (var (rt, values) in _rectPositions.ToList())
            {
                RectFullPosition rectFullPos;
                if (rt.TryGetComponent<TMP_Text>(out var text))
                    rectFullPos = ScriptableObject.CreateInstance<TextFullPosition>().Initialize(text);
                else
                    rectFullPos = ScriptableObject.CreateInstance<RectFullPosition>().Initialize(rt);

                var index = _indexForBake;
                if (values.Count == 0)
                {
                    values.RectFullPositions.Add(rectFullPos);
                }
                else if (index < 0)
                {
                    values.RectFullPositions.Add(rectFullPos);
                }
                else if (index >= values.Count)
                {
                    values.RectFullPositions.AddRange(Enumerable.Repeat(ScriptableObject.CreateInstance<RectFullPosition>(), index - values.Count));
                    values.RectFullPositions.Add(rectFullPos);
                }
                else
                {
                    var prevFullPos = values.RectFullPositions[index];
                    rectFullPos.IgnoreActiveBake = prevFullPos.IgnoreActiveBake;
                    values.RectFullPositions[index] = rectFullPos;
                }
                
                _rectPositions[rt] = values; // Addictive Serialization handle
            }

            foreach (var adaptByStateElement in _adaptByStateElements)
            {
                adaptByStateElement.BakeForIndex(_indexForBake);
            }
        }

        public void BakeForState(int index)
        {
            _indexForBake = index;
            ChangeBakedState();
        }
        
        [Button]
        private void BakeState() => ChangeBakedState();
    }
}
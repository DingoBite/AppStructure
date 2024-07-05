using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    [Serializable]
    public class FullPosList
    {
        [SerializeReference] public List<RectFullPosition> RectFullPositions = new();
        public int Count => RectFullPositions.Count;
    }
}
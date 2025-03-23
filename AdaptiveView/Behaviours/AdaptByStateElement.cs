using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public abstract class AdaptByStateElement : MonoBehaviour
    {
        public abstract void Adapt(int index);
        public abstract void BakeForIndex(int index);
    }
}
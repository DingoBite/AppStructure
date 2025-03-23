using System;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class FullPosition : ScriptableObject
    {
        [SerializeField] public bool IgnoreActivenessBake;
        [SerializeField] public bool Active = true;

        public virtual void Collect(object obj)
        {
            Active = obj as GameObject;
        }
        
        public virtual void Apply(object obj)
        {
            if (IgnoreActivenessBake || obj is not GameObject go)
                return;
            go.SetActive(Active);
        }
    }
}
using System;
using UnityEngine;

namespace AppStructure.AdaptiveView
{
    [Serializable]
    public class FullPosition : ScriptableObject
    {
        public bool IgnoreActiveBake;
        
        public bool Active = true;

        public FullPosition(GameObject gameObject)
        {
            Active = gameObject.activeSelf;
        }
        
        public FullPosition() {}

        protected void Apply(GameObject gameObject)
        {
            if (IgnoreActiveBake || gameObject == null)
                return;
            gameObject.SetActive(Active);
        }
        
        public virtual bool Apply<T>(T obj)
        {
            var isType = typeof(T) == typeof(GameObject);
            if (isType)
                Apply(obj as GameObject);
            return isType;
        }
    }
}
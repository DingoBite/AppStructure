using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public class FullPosition : ScriptableObject
    {
        [SerializeField] public bool IgnoreActiveBake;
        [SerializeField] public bool Active = true;

        public FullPosition Initialize(GameObject gameObject)
        {
            Active = gameObject.activeSelf;
            return this;
        }
        
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
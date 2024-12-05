using UnityEngine;

namespace AppStructure.Utils
{
    public abstract class SubscribableBehaviour : MonoBehaviour
    {
        protected virtual bool ForSubscriptionPrepared => true;
        
        protected abstract void SubscribeOnly();
        protected abstract void UnsubscribeOnly();

        protected virtual void OnEnable()
        {
            if (!ForSubscriptionPrepared)
                return;
            
            UnsubscribeOnly();
            SubscribeOnly();
        }

        protected virtual void OnDisable()
        {
            if (!ForSubscriptionPrepared)
                return;
            
            UnsubscribeOnly();
        }
    }
}
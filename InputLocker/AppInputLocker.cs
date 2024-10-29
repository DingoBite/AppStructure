using UnityEngine;

namespace AppStructure.InputLocker
{
    public abstract class AppInputLocker<TLockMessage> : MonoBehaviour, IAppInputLocker<TLockMessage>
    {
        protected TLockMessage LastLockMessage { get; private set; }
        protected long LockFlags { get; private set; }

        public void Initialize()
        {
            LockFlags = 0;
            Disable();
        }

        public void Enable(TLockMessage lockMessage, ushort lockFlag = 0)
        {
            LastLockMessage = lockMessage;
            LockFlags |= (uint)(1 << lockFlag);
        }

        public void Disable(ushort lockFlag = 0)
        {
            LockFlags &= ~(uint)(1 << lockFlag);
            if (LockFlags != 0)
                return;
        }

        protected abstract void OnLockEnable(TLockMessage lockMessage);
        protected abstract void OnLockDisable();
    }
}
namespace AppStructure.InputLocker
{
    public interface IAppInputLocker<in TLockMessage>
    {
        void Enable(TLockMessage lockMessage, ushort lockFlag);
        void Disable(ushort lockFlag = 0);
    }
}
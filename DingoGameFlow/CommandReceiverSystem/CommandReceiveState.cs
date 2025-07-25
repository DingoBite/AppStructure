namespace AppStructure.DingoGameFlow.CommandReceiverSystem
{
    public enum CommandReceiveState
    {
        None,
        Success,
        Pending,
        Collision,
        Canceled,
        
        Disabled,
        
        InvalidData,
        InitializeError,
        Exception,
    }
}
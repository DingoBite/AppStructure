using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppStructure.DingoGameFlow.CommandReceiverSystem
{
    public interface ICommandReceiver<in T>
    {
        UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null);
    }
}
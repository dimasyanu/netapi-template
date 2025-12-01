using System.Threading.Channels;
using MediatR;

namespace NetApi.Application.Common.Models;

public abstract class Job : IDisposable
{
    protected bool? IsCompleted { get; set; } = null;
    protected readonly Channel<bool> CompletionChannel;

    public required string Key { get; init; }

    public Job()
    {
        IsCompleted = false;
        CompletionChannel = Channel.CreateBounded<bool>(1);
    }

    /// <summary>
    /// Waits for the job to complete.
    /// </summary>
    /// <returns></returns>
    public virtual async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
    {
        // Wait for completion signal
        await CompletionChannel.Reader.ReadAsync(cancellationToken);
    }

    public void Dispose()
    {
        IsCompleted = true;
        CompletionChannel.Writer.WriteAsync(true).AsTask().GetAwaiter().GetResult();
        CompletionChannel.Writer.Complete();

        GC.SuppressFinalize(this);
    }
}

public class Job<TCommand> : Job where TCommand : IBaseRequest
{
    public required TCommand Command { get; init; }
}

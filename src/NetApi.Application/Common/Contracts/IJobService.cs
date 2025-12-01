using MediatR;
using NetApi.Application.Common.Models;

namespace NetApi.Application.Common.Contracts;

public interface IJobService
{
    Task EnqueueAsync<TCommand>(Job<TCommand> job, CancellationToken cancellationToken = default) where TCommand : IBaseRequest;

    Task<IReadOnlyList<Job>> GetQueuedJobsAsync(CancellationToken cancellationToken = default);

    Task StartAsync(CancellationToken cancellationToken = default);
}

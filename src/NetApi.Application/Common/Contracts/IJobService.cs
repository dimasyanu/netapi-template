using MediatR;

namespace NetApi.Application.Common.Contracts;

public interface IJobService
{
    Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IBaseRequest;

    IReadOnlyList<IBaseRequest> GetQueuedJobs();

    IBaseRequest? Pop();

    Task StartAsync(CancellationToken cancellationToken);
}

using MediatR;

namespace NetApi.Application.Common.Models;

public abstract class Job
{
    public required string Key { get; init; }
}

public class Job<TCommand> : Job where TCommand : IBaseRequest
{
    public required TCommand Command { get; init; }
}

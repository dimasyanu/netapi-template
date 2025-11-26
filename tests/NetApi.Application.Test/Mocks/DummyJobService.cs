using MediatR;
using NetApi.Application.Common.Contracts;
using Quartz;

namespace NetApi.Application.Test.Mocks;

public class DummyJobService(ISchedulerFactory schedulerFactory) : IJobService
{
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;
    private readonly List<IJob> _jobs = [];
    private IScheduler? _scheduler;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await _scheduler.Start(cancellationToken);
    }

    public async Task EnqueueAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IBaseRequest
    {
        var jobDetail = JobBuilder.Create<TJob>()
            .WithIdentity(Guid.NewGuid().ToString())
            .UsingJobData("CommandType", command.GetType().AssemblyQualifiedName!)
            .Build();

        _jobs.Add(command);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
    }

    public IReadOnlyList<IBaseRequest> GetQueuedJobs()
    {
        return _jobs.AsReadOnly();
    }

    public IBaseRequest? Pop()
    {
        if (_jobs.Count == 0) {
            return null;
        }

        var job = _jobs[0];
        _jobs.RemoveAt(0);
        return job;
    }
}

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Models;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace NetApi.Application.Test.Mocks;

public class DummyJobService : IJobService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IServiceProvider? _serviceProvider;
    private IScheduler? _scheduler;

    public DummyJobService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public DummyJobService(ISchedulerFactory schedulerFactory, IServiceProvider serviceProvider)
    {
        _schedulerFactory = schedulerFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        if (_scheduler.IsStarted) return;
        if (_serviceProvider != null) {
            // Use custom job factory so Quartz tries DI constructor first
            _scheduler.JobFactory = new ServiceProviderJobFactory(_serviceProvider);
        }
        await _scheduler.Start(cancellationToken);
    }

    public async Task EnqueueAsync<TCommand>(Job<TCommand> job, CancellationToken cancellationToken = default) where TCommand : IBaseRequest
    {
        var jobKey = JobKey.Create(job.Key, "Default");
        var jobDetail = JobBuilder.Create<DummyQuartzJob<TCommand>>()
            .WithIdentity(jobKey)
            .UsingJobData(new JobDataMap {
                { "Command", job.Command }
            })
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(job.Key, "Default")
            .StartNow()
            .Build();

        // var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        _scheduler ??= await _schedulerFactory.GetScheduler(cancellationToken);
        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetQueuedJobs()
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

        var results = new List<Job>();
        foreach (var key in keys) {
            var detail = await scheduler.GetJobDetail(key);
            if (detail == null
                || !detail.JobDataMap.ContainsKey("Command")
                || detail.JobDataMap.Get("Command") == null
                || detail.JobDataMap.Get("Command") is not Job)
                continue;

            var job = detail.JobDataMap.Get("Command") as Job;
            results.Add(job!);
        }
        return results;
    }
}

/// <summary>
/// A custom Quartz job factory that resolves jobs via IServiceProvider allowing DI constructors
/// </summary>
/// <param name="serviceProvider"></param>
public sealed class ServiceProviderJobFactory(IServiceProvider serviceProvider) : IJobFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobType = bundle.JobDetail.JobType;
        // Try resolving from DI; fallback to Activator if not registered

        return (IJob)Activator.CreateInstance(jobType, _serviceProvider)!;
    }

    public void ReturnJob(IJob job)
    {
        if (job is IDisposable d) d.Dispose();
    }
}

/// <summary>
/// A dummy Quartz job that executes a command of type TCommand
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public class DummyQuartzJob<TCommand> : Job<TCommand>, IJob where TCommand : IBaseRequest
{
    private readonly IServiceProvider _serviceProvider;

    // Optional DI constructor if a custom IJobFactory is later provided
    public DummyQuartzJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var command = (TCommand)context.JobDetail.JobDataMap.Get("Command")!;
        Console.WriteLine($"Executing job {Key} with command of type {typeof(TCommand).Name}");
        await mediator.Send(command);
    }
}

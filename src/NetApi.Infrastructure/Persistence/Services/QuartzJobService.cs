using MediatR;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Models;
using NetApi.Infrastructure.Persistence.Models;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace NetApi.Infrastructure.Persistence.Services;

public class QuartzJobService : IJobService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IServiceProvider? _serviceProvider;
    private IScheduler? _scheduler;

    public QuartzJobService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public QuartzJobService(ISchedulerFactory schedulerFactory, IServiceProvider serviceProvider)
    {
        _schedulerFactory = schedulerFactory;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Starts the Quartz scheduler
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        if (_scheduler.IsStarted) return;
        if (_serviceProvider != null) {
            // Use custom job factory so Quartz tries DI constructor first
            _scheduler.JobFactory = new QuartzJobFactory(_serviceProvider);
        }
        await _scheduler.Start(cancellationToken);
    }

    /// <summary>
    /// Enqueues a job to the Quartz scheduler
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="job"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task EnqueueAsync<TCommand>(Job<TCommand> job, CancellationToken cancellationToken = default) where TCommand : IBaseRequest
    {
        var jobKey = JobKey.Create(job.Key, "Default");
        var jobDetail = JobBuilder.Create<QuartzJob<TCommand>>()
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

    /// <summary>
    /// Gets the list of queued jobs
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IReadOnlyList<Job>> GetQueuedJobsAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken); // Small delay to ensure jobs are registered

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);

        var results = new List<Job>();
        foreach (var key in keys) {
            var jobExecutionContexts = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            foreach (var executingContext in jobExecutionContexts) {
                var JobDataMap = executingContext.JobDetail.JobDataMap;
                var jobInstance = executingContext.JobInstance;
                if (jobInstance is not Job
                    || !JobDataMap.ContainsKey("Command")
                    || JobDataMap.Get("Command") == null
                    || JobDataMap.Get("Command") is not ICommand)
                    continue;

                var job = jobInstance as Job;
                results.Add(job!);
            }
        }
        return results;
    }
}

/// <summary>
/// Custom Quartz job factory to integrate with Microsoft DI
/// </summary>
/// <param name="serviceProvider"></param>
public class QuartzJobFactory(IServiceProvider serviceProvider) : IJobFactory
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
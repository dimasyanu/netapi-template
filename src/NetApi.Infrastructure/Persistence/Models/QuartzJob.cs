using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Models;
using Quartz;

namespace NetApi.Infrastructure.Persistence.Models;

public class QuartzJob<TCommand>(IServiceProvider serviceProvider) : Job<TCommand>, IJob where TCommand : IBaseRequest
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task Execute(IJobExecutionContext context)
    {

        using var scope = _serviceProvider.CreateScope();
        var command = (TCommand)context.JobDetail.JobDataMap.Get("Command")!;
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(command);
    }
}

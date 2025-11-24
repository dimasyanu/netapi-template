using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users.Commands;
using NetApi.Application.Users.Queries;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests;

public class BaseIntegrationTest
{
    private readonly IServiceProvider _service;
    protected readonly ITestOutputHelper Output;

    public BaseIntegrationTest(ITestOutputHelper output)
    {
        Output = output;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContext<AppDbContext>(options => {
            options.UseInMemoryDatabase("TestDb");
        });
        serviceCollection.AddLogging();
        serviceCollection.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<GetUserByIdQueryHandler>());
        serviceCollection.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<CreateUserCommandHandler>());

        ConfigureServices(serviceCollection);
        _service = serviceCollection.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected T GetService<T>() where T : notnull
    {
        return _service.GetRequiredService<T>();
    }
}

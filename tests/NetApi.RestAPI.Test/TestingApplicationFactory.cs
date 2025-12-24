using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Infrastructure.Persistence;

namespace NetApi.RestAPI.Test;

public class TestingApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("NetApiInMemoryDb"));
        });
    }
}

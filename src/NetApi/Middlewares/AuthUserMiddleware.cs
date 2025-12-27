using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Users;
using NetApi.Constants;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Middlewares;

public class AuthUserMiddleware(IServiceProvider serviceProvider) : IMiddleware
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() == null) {
            await next.Invoke(context);
            return;
        }

        if (!(context.User.Identity?.IsAuthenticated ?? false))
            throw new UnauthorizedException();

        var email = context.User.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedException();

        using (var scope = _serviceProvider.CreateScope()) {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var userEntity = await userRepo.GetByEmailAsync(EmailAddress.FromString(email), null, cancellationToken)
                ?? throw new UnauthorizedException();

            context.Items.Add(AuthConstant.CURRENT_USER_KEY, User.FromEntity(userEntity));
        }

        await next.Invoke(context);
    }
}

public static class AuthUserMiddlewareExtensions
{
    public static IServiceCollection AddAuthUser(this IServiceCollection services)
    {
        return services.AddScoped<AuthUserMiddleware>();
    }

    public static IApplicationBuilder UseAuthUserMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthUserMiddleware>();
    }
}

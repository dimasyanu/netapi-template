using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Attributes;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Media;
using NetApi.Application.Roles;
using NetApi.Domain.Users;

namespace NetApi.Application.Common.PipelineBehaviors;

public class AuthorizedRequestBehavior<TRequest, TResponse>(
    IServiceProvider serviceProvier,
    IRolePermissionRepository permissionRepo
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    private readonly IServiceProvider _serviceProvider = serviceProvier;
    private readonly IRolePermissionRepository _permissionRepo = permissionRepo;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next(cancellationToken);

        if (!authorizedRequest.IsAuthenticated())
            throw new UnauthorizedException();

        var roles = authorizedRequest.User!.Roles;
        if (roles?.Any(x => x.IsSuperAdmin) ?? false)
            return await next(cancellationToken);

        var roleIds = roles?.Select(x => x.Id!) ?? throw new UnauthorizedException();
        (string feature, byte action, bool restrictOwnership) = authorizedRequest.GetRequestPermission();
        if (!await _permissionRepo.CheckAccessAsync(feature, action, roleIds, cancellationToken))
            throw new UnauthorizedException();

        if (restrictOwnership && !await CheckOwnership(feature, action, authorizedRequest))
            throw new UnauthorizedException();

        return await next(cancellationToken);
    }

    private async Task<bool> CheckOwnership(string feature, byte action, IAuthorizedRequest authorizedRequest)
    {
        var user = authorizedRequest.User!;

        // Get resource Ids
        var resourceIds = new List<object>();
        var props = authorizedRequest.GetType().GetProperties();
        var singleKey = props
            .Where(x => x.GetCustomAttribute<ResourceKeyAttribute>() != null)
            .Select(x => x.GetValue(authorizedRequest))
            .Where(x => x != null)
            .Select(x => x!);
        resourceIds.AddRange(singleKey);

        var someKeys = props
            .Where(x => x.GetCustomAttribute<ResourceKeysAttribute>() != null)
            .Select(x => {
                var val = x.GetValue(authorizedRequest);
                if (val is not System.Collections.IEnumerable enumerable)
                    throw new InternalErrorException($"Invalid resource keys for feature '{feature}:{action}'.");
                return enumerable;
            })
            .Where(x => x != null)
            .Select(x => x!);
        foreach (var keys in someKeys)
            resourceIds.AddRange(keys);

        using var scope = _serviceProvider.CreateScope();
        if (feature.Equals(Feature.Media)) {
            var repo = scope.ServiceProvider.GetRequiredService<IMediaRepository>();
            return await repo.CheckOwnershipAsync(resourceIds, user.Id!);
        }
        throw new InternalErrorException($"Ownership handler for feature '{feature}:{action}' is not defined.");
    }
}


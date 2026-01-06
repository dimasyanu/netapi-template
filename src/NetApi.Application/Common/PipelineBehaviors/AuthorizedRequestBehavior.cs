using MediatR;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Roles;

namespace NetApi.Application.Common.PipelineBehaviors;

public class AuthorizedRequestBehavior<TRequest, TResponse>(IRolePermissionRepository permissionRepo) : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
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
        (string feature, byte action) = authorizedRequest.GetRequestPermission();
        if (!await _permissionRepo.CheckAccessAsync(feature, action, roleIds, cancellationToken))
            throw new UnauthorizedException();

        return await next(cancellationToken);
    }
}


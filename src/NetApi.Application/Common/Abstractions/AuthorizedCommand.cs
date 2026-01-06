using NetApi.Application.Common.Attributes;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users;
using System.Reflection;

namespace NetApi.Application.Common.Abstractions;

public abstract class AuthorizedCommand<TResult> : IAuthorizedRequest, ICommand<TResult>
{
    public User? User { get; init; }

    public bool IsAuthenticated()
        => User != null && User.Id != null && !User.Id.IsEmpty();

    public (string, byte) GetRequestPermission()
    {
        var type = GetType();
        var attribute = type.GetCustomAttribute<PermissionAttribute>()
            ?? throw new InternalErrorException($"{type.Name} is not implementing any permission.");

        return (attribute.Feature, attribute.Action);
    }
}


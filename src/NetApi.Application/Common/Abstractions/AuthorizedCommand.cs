using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Common.Abstractions;

public abstract class AuthorizedCommand<TResult> : ICommand<TResult>
{
    public User? User { get; init; }
}

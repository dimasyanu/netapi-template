using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Common.Contracts;

public interface IHasOwnershipRepository
{
    Task<bool> CheckOwnershipAsync(IEnumerable<object> ids, UserId userId);
}

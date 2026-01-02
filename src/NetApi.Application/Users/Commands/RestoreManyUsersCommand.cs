using NetApi.Application.Common.Abstractions;

namespace NetApi.Application.Users.Commands;

public class RestoreManyUsersCommand : AuthorizedCommand<bool>
{
    public IEnumerable<Guid> Ids { get; set; } = [];
}

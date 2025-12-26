namespace NetApi.Application.Users.Commands;

public class TrashManyUsersCommand(IEnumerable<Guid> ids)
{
    public IEnumerable<Guid> Ids { get; set; } = ids;
}

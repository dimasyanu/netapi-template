using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class SaveUserSettingsCommand : AuthorizedCommand<bool>
{
    public required UserId UserId { get; set; }
    public required UserSetting UserSettings { get; set; }
}

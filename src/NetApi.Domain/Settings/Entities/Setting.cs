using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Settings.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Settings.Entities;

public class Setting : IEntity<SettingId>
{
    public SettingId Id { get; set; } = SettingId.Create();

    public string Key { get; set; } = "";
    public string Value { get; set; } = "";

    public UserId? OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
}

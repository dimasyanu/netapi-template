using System.Reflection;
using System.Text.Json;
using NetApi.Domain.Settings;
using NetApi.Domain.Users.Entities;

namespace NetApi.Domain.Users;

public class UserSetting
{
    [UserSettingKey("theme")]
    public string Theme { get; set; } = "light";

    [UserSettingKey("language")]
    public string DefaultLanguage { get; set; } = "en";

    [UserSettingKey]
    public bool ReceiveNewsletter { get; set; } = true;

    public static UserSetting? FromEntities(IReadOnlyList<UserSettingEntity>? entities)
    {
        if (entities == null || entities.Count == 0) {
            return null;
        }

        var setting = new UserSetting();
        var props = typeof(UserSetting).GetProperties()
            .Select(x => KeyValuePair.Create(x, x.GetCustomAttribute<UserSettingKeyAttribute>()))
            .Where(x => x.Value != null)
            .ToList();

        foreach (var (prop, attr) in props) {
            var key = attr!.GetKey(prop.Name);
            var entity = entities.FirstOrDefault(x => x.Key == key);
            if (entity == null) continue;
            prop.SetValue(setting, entity.Value == null ? null : JsonSerializer.Deserialize(entity.Value, prop.PropertyType));
        }

        return setting;
    }

    public IReadOnlyList<UserSettingEntity> ToEntities()
    {
        var entities = new List<UserSettingEntity>();
        var props = typeof(UserSetting).GetProperties()
            .Select(x => KeyValuePair.Create(x, x.GetCustomAttribute<UserSettingKeyAttribute>()))
            .Where(x => x.Value != null)
            .ToList();

        foreach (var (prop, attr) in props) {
            var key = attr!.GetKey(prop.Name);
            var value = prop.GetValue(this);
            var entity = new UserSettingEntity {
                Key = key,
                Value = value == null ? null : JsonSerializer.Serialize(value)
            };
            entities.Add(entity);
        }

        return entities;
    }
}

namespace NetApi.Domain.Settings;

[AttributeUsage(AttributeTargets.Property)]
public class UserSettingKeyAttribute(string? key = null) : Attribute
{
    public string? Key { get; } = key;

    public string GetKey(string propertyName)
    {
        // If Key is null, use the slugified property name as the key
        return Key ?? CreateSlug(propertyName);
    }

    private static string CreateSlug(string value)
    {
        return string.Concat(
            value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLower(x) : char.ToLower(x).ToString())
        );
    }
}

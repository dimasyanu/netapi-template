using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Settings.ValueObjects;

public class SettingId : ValueObject, ILongObject
{
    private readonly long _value;
    public long Value => _value;

    private SettingId(long val)
    {
        _value = val;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _value;
    }

    public static SettingId Create() => new(0);
    public static SettingId FromLong(long val) => new(val);
}

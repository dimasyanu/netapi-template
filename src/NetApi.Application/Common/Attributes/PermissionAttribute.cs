namespace NetApi.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PermissionAttribute(string feature, byte action) : Attribute
{
    public readonly string Feature = feature;
    public readonly byte Action = action;
}


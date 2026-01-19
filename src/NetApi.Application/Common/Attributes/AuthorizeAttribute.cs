namespace NetApi.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AuthorizeAttribute(string feature, byte action, bool restrictOwnership = false) : Attribute
{
    public readonly string Feature = feature;
    public readonly byte Action = action;
    public readonly bool RestrictOwnership = restrictOwnership;
}

namespace NetApi.Domain.Common.Constants;

public static class RoleConstant
{
    public const string FeatureName = "roles";

    public static class Permission
    {
        public const byte Read = 1;
        public const byte Create = 2;
        public const byte Update = 3;
        public const byte Delete = 4;
    }
}


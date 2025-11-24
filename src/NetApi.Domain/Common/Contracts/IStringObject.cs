namespace NetApi.Domain.Common.Contracts;

public interface IStringObject
{
    string ToString();
}

public static class StringObjectExtensions
{
    public static string ToLower(this IStringObject stringObject)
    {
        return stringObject.ToString().ToLower();
    }

    public static string ToUpper(this IStringObject stringObject)
    {
        return stringObject.ToString().ToUpper();
    }

    public static bool IsNullOrEmpty(this IStringObject stringObject)
    {
        return string.IsNullOrEmpty(stringObject.ToString());
    }
}

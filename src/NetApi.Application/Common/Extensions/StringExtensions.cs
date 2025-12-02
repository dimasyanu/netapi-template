namespace NetApi.Application.Common.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < str.Length; i++) {
            var c = str[i];
            if (char.IsUpper(c)) {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLower(c));
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
}

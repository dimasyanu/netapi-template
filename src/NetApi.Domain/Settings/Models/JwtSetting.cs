namespace NetApi.Domain.Settings.Models;

public class JwtSetting
{
    public string SecretKey { get; set; } = "";
    public int ExpiryInMinutes { get; set; } = 60;
}

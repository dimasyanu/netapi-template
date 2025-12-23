namespace NetApi.Infrastructure.Persistence.Models;

public class AppSettings
{
    public string AppName { get; set; } = "NetApi";
    public string AppVersion { get; set; } = "0.0.0";
    public SmtpSetting? SmtpSettings { get; set; }
    public JwtSettings? Jwt { get; set; } = new JwtSettings();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public int AuthKeyLengthInBytes { get; set; } = 32; // 256 bits
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenValidityInDays { get; set; } = 5;
}

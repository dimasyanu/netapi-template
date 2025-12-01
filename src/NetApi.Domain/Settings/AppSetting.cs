using NetApi.Domain.Settings.Models;

namespace NetApi.Domain.Settings;

public class AppSetting
{
    public string AppName { get; set; } = "NetApi";
    public string AppVersion { get; set; } = "0.0.0";
    public SmtpSetting? SmtpSettings { get; set; }
    public JwtSetting? JwtSettings { get; set; }
}

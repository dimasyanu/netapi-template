using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Settings.Models;

public class SmtpSetting
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool? EnableSsl { get; set; }

    public EmailAddress? From { get; set; }
}

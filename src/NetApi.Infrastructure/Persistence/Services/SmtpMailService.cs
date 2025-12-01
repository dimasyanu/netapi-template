using System.Net;
using System.Net.Mail;
using NetApi.Application.Common.Contracts;
using NetApi.Domain.Settings;
using NetApi.Domain.Users.ValueObjects;
using Quartz.Impl.AdoJobStore;

namespace NetApi.Infrastructure.Persistence.Services;

public class SmtpMailService(AppSetting appSetting) : IMailService
{
    private readonly AppSetting _appSetting = appSetting;

    public async Task<bool> SendAsync(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null)
    {
        var smtpSettings = _appSetting.SmtpSettings
            ?? throw new InvalidConfigurationException("SMTP settings are not configured.");
        if (smtpSettings.Host == null) throw new InvalidConfigurationException("SMTP Host is not configured.");
        if (smtpSettings.Port == null) throw new InvalidConfigurationException("SMTP Port is not configured.");
        if (smtpSettings.Username == null) throw new InvalidConfigurationException("SMTP Username is not configured.");
        if (smtpSettings.Password == null) throw new InvalidConfigurationException("SMTP Password is not configured.");
        if (smtpSettings.From == null) throw new InvalidConfigurationException("SMTP From address is not configured.");

        var client = new SmtpClient(smtpSettings.Host) {
            Port = smtpSettings.Port.Value,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
            EnableSsl = smtpSettings.EnableSsl ?? true,
        };

        // Create mail message
        var mailMessage = new MailMessage {
            From = new MailAddress(smtpSettings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        // Add to, cc and bcc addresses
        AddMailAddresses(mailMessage.To, to);
        AddMailAddresses(mailMessage.CC, cc);
        AddMailAddresses(mailMessage.Bcc, bcc);

        var t = DateTime.Now;
        await client.SendMailAsync(mailMessage);
        var msg = "SMTP Email sent successfully. Elapsed time: " + (DateTime.Now - t).Seconds + " s";
        Console.WriteLine(msg);

        return true;
    }

    private static void AddMailAddresses(MailAddressCollection mailAddresses, EmailAddress[]? cc)
    {
        if (cc == null || cc.Length < 1) return;

        foreach (var ccAddress in cc) {
            mailAddresses.Add(ccAddress.ToString());
        }
    }
}


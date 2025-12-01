using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NetApi.Application.Test.Mocks.Models;
using Quartz.Impl.AdoJobStore;

namespace NetApi.Application.Test.Mocks;

public class DummyMailtrapClient(IConfiguration configuration)
{
    private const string HostBase = "https://mailtrap.io/api/accounts/{account_id}";
    private string AccountId => configuration["Mailtrap:AccountId"]
        ?? throw new InvalidConfigurationException("Mailtrap:AccountId is not configured");

    private string InboxId => configuration["Mailtrap:InboxId"]
       ?? throw new InvalidConfigurationException("Mailtrap:InboxId is not configured");

    private string ApiToken => configuration["Mailtrap:ApiToken"]
        ?? throw new InvalidConfigurationException("Mailtrap:ApiToken is not configured");

    private string Host => HostBase.Replace("{account_id}", AccountId);

    public async Task<List<MailtrapMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        var requestUri = $"{Host}/inboxes/{InboxId}/messages";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Api-Token", ApiToken);

        var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var messages = JsonSerializer.Deserialize<List<MailtrapMessage>>(content) ?? new();

        return messages;
    }

    public async Task<bool> CleanInboxAsync(CancellationToken cancellationToken = default)
    {
        var requestUri = $"{Host}/inboxes/{InboxId}/clean";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Api-Token", ApiToken);

        var response = await httpClient.PatchAsync(requestUri, null, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

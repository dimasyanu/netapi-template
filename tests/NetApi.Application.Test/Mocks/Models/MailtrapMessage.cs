using System.Text.Json.Serialization;

namespace NetApi.Application.Test.Mocks.Models;

public class MailtrapMessage
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("from_email")]
    public string FromEmail { get; set; } = string.Empty;

    [JsonPropertyName("from_name")]
    public string FromName { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

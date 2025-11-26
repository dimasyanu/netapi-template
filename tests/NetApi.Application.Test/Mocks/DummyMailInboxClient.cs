using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Test.Mocks;

public class DummyMailInboxClient(IMailService mailService)
{
    public async Task<IReadOnlyList<EmailMessage>> GetInboxAsync(EmailAddress address)
    {
        var dummyMailService = mailService as DummyMailService
            ?? throw new InvalidOperationException("Mail service is not a DummyMailService");
        return await dummyMailService.GetInboxAsync(address);
    }
}

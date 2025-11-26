using NetApi.Application.Common.Contracts;

namespace NetApi.Infrastructure.Persistence.Services;

public class HashingService : IHashingService
{
    public string GenerateSecureToken(int length = 32)
    {
        throw new NotImplementedException();
    }

    public string HashPassword(string password, string? salt = null)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword, string? salt = null)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

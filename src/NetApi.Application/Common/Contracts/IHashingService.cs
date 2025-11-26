namespace NetApi.Application.Common.Contracts;

public interface IHashingService
{
    string HashPassword(string password, string? salt = null);
    bool VerifyPassword(string password, string hashedPassword, string? salt = null);
    string GenerateSecureToken(int length = 32);
}

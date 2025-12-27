using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetApi.Application.Auth;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Users;
using NetApi.Domain.Auth.Models;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence.Models;

namespace NetApi.Infrastructure.Persistence.Services;

public class JwtAuthService(IUserRepository userRepo, IHashingService hashingService, AppSettings appSettings) : IAuthService
{
    private readonly IUserRepository _userRepo = userRepo;
    private readonly IHashingService _hashingService = hashingService;
    private readonly JwtSettings _jwtSettings = appSettings.Jwt
        ?? throw new InvalidConfigurationException("JWT settings are not configured.");

    /// <summary>
    /// Attempts to log in a user with the provided email and password.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<User> AttemptLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var userEntity = await _userRepo.GetByEmailAsync(EmailAddress.FromString(email), [], cancellationToken)
            ?? throw new BadRequestException("Invalid email or password.");

        if (!_hashingService.VerifyPassword(password, userEntity.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        return User.FromEntity(userEntity);
    }

    /// <summary>
    /// Generates authentication tokens for the given user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<LoginResult> GenerateAuthTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var issuer = _jwtSettings.Issuer ?? throw new InvalidConfigurationException("JWT Issuer is not defined");
        var audience = _jwtSettings.Audience ?? throw new InvalidConfigurationException("JWT Audience is not defined");
        var secretKey = _jwtSettings.SecretKey ?? throw new InvalidConfigurationException("JWT Secret Key is not defined");

        // Generate access token
        var secret = Encoding.UTF8.GetBytes(secretKey);
        var tokenValidityMinutes = _jwtSettings.AccessTokenExpirationMinutes;
        var tokenExpiration = DateTime.Now.AddMinutes(tokenValidityMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Email, user.EmailAddress.ToString())]),
            Expires = tokenExpiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // Generate refresh token
        var bytes = new byte[_jwtSettings.AuthKeyLengthInBytes];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(bytes);
        }
        var refreshToken = Convert.ToBase64String(bytes);

        // Store refresh token and its expiry time in the database
        await _userRepo.UpdateAsync(user.Id!, u => {
            u.RefreshToken = refreshToken;
            u.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtSettings.RefreshTokenValidityInDays);
        }, cancellationToken);

        return new LoginResult {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}

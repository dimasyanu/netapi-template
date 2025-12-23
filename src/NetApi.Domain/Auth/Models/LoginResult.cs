namespace NetApi.Domain.Auth.Models;

public class LoginResult
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}

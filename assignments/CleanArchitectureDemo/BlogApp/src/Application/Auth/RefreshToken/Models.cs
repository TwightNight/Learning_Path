namespace BlogApp.Application.Auth.RefreshToken;


// BlogApp.Web/Endpoints/Auth/RefreshTokenRequest.cs
public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
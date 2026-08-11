namespace BlogApp.Application.Auth.Regis;

public class RegisRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
public class RegisResponse
{
    public string? Email {get; set;}
    // public string UserName { get; set; } = default!;
    // public string AccessToken { get; set; } = string.Empty;
    // public DateTime AccessTokenExpiry { get; set; }
    // public string RefreshToken { get; set; } = string.Empty;
    // public DateTime RefreshTokenExpiry { get; set; }

}


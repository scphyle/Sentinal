namespace Sentinal.Api.Models.Requests;

public class LoginUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
}
namespace Service.Models.Auth;

public class AuthRequestModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
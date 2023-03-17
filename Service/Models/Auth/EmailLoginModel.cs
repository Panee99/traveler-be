namespace Service.Models.Auth;

public class EmailLoginModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
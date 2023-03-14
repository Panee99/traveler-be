namespace Data.Models.Chat;

public class TokenResponseModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
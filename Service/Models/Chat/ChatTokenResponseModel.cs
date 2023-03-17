namespace Service.Models.Chat;

public class ChatTokenResponseModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
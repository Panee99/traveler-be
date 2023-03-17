namespace Service.Models.Chat;

public record ChatTokenResponseModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
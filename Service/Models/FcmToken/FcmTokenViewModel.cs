namespace Service.Models.FcmToken;

public record FcmTokenViewModel
{
    public Guid Id;
    public string Token = "";
    public Guid UserId;
}
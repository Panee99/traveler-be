namespace Service.Models.Chat;

public record ChatTokenResponseModel
(
    Guid UserId,
    Guid GroupId,
    string Token
);
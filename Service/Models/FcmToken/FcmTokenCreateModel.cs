namespace Service.Models.FcmToken;

public record FcmTokenCreateModel
(
    string Token,
    Guid UserId
);
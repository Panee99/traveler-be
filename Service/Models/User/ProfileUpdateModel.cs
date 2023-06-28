namespace Service.Models.User;

public record ProfileUpdateModel
(
    string? FirstName,
    string? LastName,
    string? Gender,
    Guid? AvatarId
);
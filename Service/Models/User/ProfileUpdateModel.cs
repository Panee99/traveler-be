namespace Service.Models.User;

public record ProfileUpdateModel
(
    string? BankName,
    string? BankNumber,
    //
    string? FirstName,
    string? LastName,
    string? Birthday,
    string? Gender,
    string? Address,
    Guid? AvatarId
);
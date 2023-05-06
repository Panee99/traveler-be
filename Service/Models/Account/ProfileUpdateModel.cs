namespace Service.Models.Account;

public record ProfileUpdateModel
(
    string? BankName,
    string? BankAccountNumber,
    //
    string? FirstName,
    string? LastName,
    string? Birthday,
    string? Gender,
    string? Address,
    Guid? AvatarId
);
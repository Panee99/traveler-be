using Data.Enums;

namespace Service.Models.User;

public record UserCreateModel
(
    string Phone,
    string? Email,
    string Password,
    UserRole Role,
    UserStatus Status,
    string FirstName,
    string LastName,
    Gender Gender,
    string? AvatarUrl
);
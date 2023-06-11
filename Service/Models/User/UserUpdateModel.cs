using Data.Enums;

namespace Service.Models.User;

public record UserUpdateModel
(
    string? Phone,
    string? Email,
    string? Password,
    // UserRoleUserRole? Role,
    UserStatus? Status,
    string? FirstName,
    string? LastName,
    Gender? Gender
);
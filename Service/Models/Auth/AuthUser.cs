using Data.Enums;

namespace Service.Models.Auth;

public record AuthUser(
    Guid Id,
    AccountRole Role
);
using Shared.Enums;

namespace Application.Configurations.Auth;

public record AuthUser(
    Guid Id,
    UserRole Role
);
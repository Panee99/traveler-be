namespace Shared.Settings;

public record AppSettings
(
    string JwtSecret,
    string JwtIssuer,
    string JwtAudience
);
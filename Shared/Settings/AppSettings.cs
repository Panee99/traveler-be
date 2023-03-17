namespace Shared.Settings;

public class AppSettings
{
    public string JwtSecret { get; set; } = null!;
    public string JwtIssuer { get; set; } = null!;
    public string JwtAudience { get; set; } = null!;
}
namespace Shared.Settings;

public record VnPaySettings
(
    string BaseUrl,
    string IpnUrl,
    string ReturnUrl,
    string TmnCode,
    string HashSecret
);
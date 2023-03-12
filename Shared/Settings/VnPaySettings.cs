namespace Shared.Settings;

public class VnPaySettings
{
    public string BaseUrl { get; set; } = "";
    public string IpnUrl { get; set; } = "";
    public string ReturnUrl { get; set; } = "";
    public string TmnCode { get; set; } = "";
    public string HashSecret { get; set; } = "";
}
namespace Shared.Settings;

public class VnPaySettings
{
    public string BaseUrl { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string TmnCode { get; set; } = null!;
    public string HashSecret { get; set; } = null!;
}
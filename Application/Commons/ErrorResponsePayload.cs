namespace Application.Commons;

public class ErrorResponsePayload
{
    public DateTime Timestamp { get; set; }
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public IDictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
}
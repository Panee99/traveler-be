namespace Application.Commons;

public class ErrorResponsePayload
{
    public DateTime Timestamp { get; set; }
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public ICollection<string> Details { get; set; } = new List<string>();
}
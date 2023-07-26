namespace Service.Models.Weather;

public record WeatherAlertViewModel
{
    public Guid Id;
    public Guid TripId;
    public string Headline = null!;
    public string Urgency = null!;
    public string Severity = null!;
    public string Areas = null!;
    public string Certainty = null!;
    public string Event = null!;
    public string Note = null!;
    public DateTime Effective;
    public DateTime Expires;
    public string Description = null!;
    public string Instruction = null!;
}
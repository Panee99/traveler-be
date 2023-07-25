namespace Data.Entities;

public class WeatherAlert
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public string Headline { get; set; } = null!;

    public string Urgency { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public string Areas { get; set; } = null!;

    public string Certainty { get; set; } = null!;

    public string Event { get; set; } = null!;

    public string Note { get; set; } = null!;

    public DateTime Effective { get; set; }

    public DateTime Expires { get; set; }

    public string Description { get; set; } = null!;

    public string Instruction { get; set; } = null!;

    public Trip Trip { get; set; } = null!;
}
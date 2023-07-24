namespace Data.Entities;

public class WeatherForecast
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public DateTime DateTime { get; set; }

    public string Location { get; set; } = null!;

    public string Country { get; set; } = null!;

    public float Temperature { get; set; }

    public long IsDay { get; set; }

    public double WindKph { get; set; }

    public long Humidity { get; set; }

    public bool WillItRain { get; set; }

    public long ChanceOfRain { get; set; }

    public bool WillItSnow { get; set; }

    public long ChanceOfSnow { get; set; }

    public long VisKm { get; set; }

    public long Uv { get; set; }

    public string Condition { get; set; } = null!;

    public string IconUrl { get; set; } = null!;

    public Trip Trip { get; set; } = null!;
}
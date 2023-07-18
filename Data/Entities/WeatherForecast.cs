namespace Data.Entities;

public class WeatherForecast
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public DateTime Date { get; set; }

    public string Location { get; set; } = null!;

    public string Country { get; set; } = null!;

    public float MinTemp { get; set; }

    public float MaxTemp { get; set; }

    public float AvgTemp { get; set; }

    public float AvgHumidity { get; set; }

    public bool IsRain { get; set; }

    public float ChanceOfRain { get; set; }

    public bool IsSnow { get; set; }

    public float ChanceOfSnow { get; set; }

    public int UvIndex { get; set; }

    public string Condition { get; set; } = null!;

    public string IconUrl { get; set; } = null!;

    public Trip Trip { get; set; } = null!;
}
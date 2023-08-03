namespace Data.Entities;

public class WeatherForecast
{
    public Guid Id { get; set; }

    public Guid ScheduleId { get; set; }

    public Guid TripId { get; set; }

    public string Region { get; set; } = null!;

    public float MaxTemp { get; set; }

    public float MinTemp { get; set; }

    public float Humidity { get; set; }

    public bool WillItRain { get; set; }

    public int ChanceOfRain { get; set; }

    public bool WillItSnow { get; set; }

    public int ChanceOfSnow { get; set; }

    public float UvIndex { get; set; }

    public string Condition { get; set; } = null!;

    public string Icon { get; set; } = null!;

    public DateTime Date { get; set; }
    
    public Schedule Schedule { get; set; } = null!;

    public Trip Trip { get; set; } = null!;
}
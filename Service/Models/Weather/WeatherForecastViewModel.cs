namespace Service.Models.Weather;

public record WeatherForecastViewModel
{
    public Guid Id;
    public Guid TripId;
    public DateTime DateTime;
    public string Location = null!;
    public string Country = null!;
    public float Temperature;
    public long IsDay;
    public double WindKph;
    public long Humidity;
    public bool WillItRain;
    public long ChanceOfRain;
    public bool WillItSnow;
    public long ChanceOfSnow;
    public long VisKm;
    public long Uv;
    public string Condition = null!;
    public string IconUrl = null!;
}
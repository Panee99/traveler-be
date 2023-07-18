namespace Service.Models.Weather;

public class WeatherForecastViewModel
{
    public Guid Id;
    public Guid TripId;
    public DateTime Date;
    public string Location = null!;
    public string Country = null!;
    public float MinTemp;
    public float MaxTemp;
    public float AvgTemp;
    public float AvgHumidity;
    public bool IsRain;
    public float ChanceOfRain;
    public bool IsSnow;
    public float ChanceOfSnow;
    public int UvIndex;
    public string Condition = null!;
    public string IconUrl = null!;
}
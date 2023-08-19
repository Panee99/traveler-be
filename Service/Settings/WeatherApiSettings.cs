namespace Service.Settings;

public class WeatherApiSettings
{
    public string ApiKey { get; set; } = null!;
    public int ForecastRange { get; set; }
}
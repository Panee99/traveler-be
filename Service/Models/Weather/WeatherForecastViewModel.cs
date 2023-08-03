namespace Service.Models.Weather;

public record WeatherForecastViewModel
{
    public Guid Id;
    public Guid ScheduleId;
    public string Region = null!;
    public float MaxTemp;
    public float MinTemp;
    public float Humidity;
    public bool WillItRain;
    public int ChanceOfRain;
    public bool WillItSnow;
    public int ChanceOfSnow;
    public float UvIndex;
    public string Condition = null!;
    public string Icon = null!;
    public DateTime Date;
}
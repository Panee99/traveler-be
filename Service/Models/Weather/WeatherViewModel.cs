namespace Service.Models.Weather;

public class WeatherViewModel
{
    public ICollection<WeatherForecastViewModel> Forecasts = new List<WeatherForecastViewModel>();
    public ICollection<WeatherAlertViewModel> Alerts = new List<WeatherAlertViewModel>();
}
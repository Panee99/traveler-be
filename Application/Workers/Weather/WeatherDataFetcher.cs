using Data.EFCore;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Service.Settings;

namespace Application.Workers.Weather;

public class WeatherDataFetcher
{
    private readonly WeatherApiSettings _settings;

    public WeatherDataFetcher(IOptions<WeatherApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<(List<WeatherForecast>, List<WeatherAlert>)> GetTripWeather(
        DateTime startDate, Guid tripId, UnitOfWork unitOfWork)
    {
        // Fetch schedules
        var schedules = await unitOfWork.Trips.Query()
            .Where(trip => trip.Id == tripId)
            .SelectMany(trip => trip.Tour.Schedules)
            .ToListAsync();

        // Get weather for each schedule location
        var httpClient = new HttpClient();
        var weatherModels = new List<(WeatherForecastModel WeatherForecastModel, Guid ScheduleId)>();

        foreach (var schedule in schedules)
        {
            Console.WriteLine(schedule.Sequence);
            var response = httpClient.GetAsync(
                "https://api.weatherapi.com/v1/forecast.json" +
                $"?key={_settings.ApiKey}" +
                $"&q={schedule.Latitude},{schedule.Longitude}" +
                $"&days={_settings.ForecastRange}" +
                "&aqi=no" +
                "&alerts=yes").Result;

            if (!response.IsSuccessStatusCode) continue;

            var model = JsonConvert.DeserializeObject<WeatherForecastModel>(
                response.Content.ReadAsStringAsync().Result
            );

            if (model is null) continue;

            var forecastDate = startDate.AddDays(schedule.DayNo - 1);
            var endForecastDate = forecastDate.AddDays(1);

            // filter out ForecastDate
            model.Forecast.Forecastday = model.Forecast.Forecastday
                .Where(x => x.Date == DateOnly.FromDateTime(forecastDate))
                .ToList();

            // filter out Alerts
            model.Alerts.Alert = model.Alerts.Alert
                .Where(alert => !((alert.Effective < forecastDate && alert.Expires < forecastDate) ||
                                  (alert.Effective > endForecastDate && alert.Expires > endForecastDate)))
                .ToList();

            weatherModels.Add((model, schedule.Id));
        }

        // Mapping and return
        var weatherAlerts = weatherModels.SelectMany(x => x.WeatherForecastModel.Alerts.Alert)
            .Select(alert => new WeatherAlert
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Areas = alert.Areas,
                Effective = alert.Effective.DateTime,
                Expires = alert.Expires.DateTime,
                Certainty = alert.Certainty,
                Description = alert.Desc,
                Headline = alert.Headline,
                Event = alert.Event,
                Instruction = alert.Instruction,
                Note = alert.Note,
                Severity = alert.Severity,
                Urgency = alert.Urgency,
            }).ToList();

        var weatherForecasts = weatherModels
            .Where(x => x.WeatherForecastModel.Forecast.Forecastday.Count > 0)
            .Select(x =>
            {
                var forecastDay = x.WeatherForecastModel.Forecast.Forecastday.FirstOrDefault()!;
                var forecastDayModel = forecastDay.Day;

                return new WeatherForecast
                {
                    Id = Guid.NewGuid(),
                    TripId = tripId,
                    ScheduleId = x.ScheduleId,
                    Region = x.WeatherForecastModel.Location.Name,
                    Condition = forecastDayModel.Condition.Text,
                    Icon = forecastDayModel.Condition.Icon,
                    MinTemp = forecastDayModel.MintempC,
                    MaxTemp = forecastDayModel.MaxtempC,
                    Humidity = forecastDayModel.AvgHumidity,
                    WillItRain = forecastDayModel.DailyWillItRain,
                    WillItSnow = forecastDayModel.DailyWillItSnow,
                    ChanceOfRain = forecastDayModel.DailyChanceOfRain,
                    ChanceOfSnow = forecastDayModel.DailyChanceOfSnow,
                    UvIndex = forecastDayModel.Uv,
                    Date = forecastDay.Date.ToDateTime(TimeOnly.MinValue),
                };
            }).ToList();

        return (weatherForecasts, weatherAlerts);
    }
}
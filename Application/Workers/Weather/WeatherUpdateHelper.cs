using Data.EFCore;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Application.Workers.Weather;

public class WeatherUpdateHelper
{
    const string ApiKey = "e9a560f9a61e445193792558231607";
    const int ForecastDays = 3;

    //
    private static readonly HttpClient HttpClient = new();

    public static async Task<(List<WeatherForecast>, List<WeatherAlert>)> GetTripWeather(
        DateTime startDate,
        Guid tripId,
        UnitOfWork unitOfWork)
    {
        // Fetch schedules
        var schedules = await unitOfWork.Trips.Query()
            .Where(trip => trip.DeletedById == null)
            .Where(trip => trip.Id == tripId)
            .SelectMany(trip => trip.Tour.Schedules)
            .ToListAsync();

        // Get weather for each schedule location
        var weatherModels = schedules.Select(schedule =>
        {
            Console.WriteLine(schedule.DayNo);

            var response = HttpClient.GetAsync(
                "https://api.weatherapi.com/v1/forecast.json" +
                $"?key={ApiKey}" +
                $"&q={schedule.Latitude},{schedule.Longitude}" +
                $"&days={ForecastDays}" +
                "&aqi=no" +
                "&alerts=yes").Result;

            if (!response.IsSuccessStatusCode)
                throw new Exception("Weather forecast request failed");

            var model = JsonConvert.DeserializeObject<WeatherForecastModel>(
                response.Content.ReadAsStringAsync().Result
            );

            if (model is null)
                throw new Exception("Convert Weather model failed");

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

            return (WeatherForecastModel: model, ScheduleId: schedule.Id);
        }).ToList();

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
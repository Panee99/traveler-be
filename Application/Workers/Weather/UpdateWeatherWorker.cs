using Data.EFCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Helpers;

namespace Application.Workers.Weather;

public class UpdateWeatherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateWeatherWorker> _logger;

    public UpdateWeatherWorker(IServiceProvider serviceProvider, ILogger<UpdateWeatherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Executing Weather Update");
        return;

        // Get dependencies
        var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        // Fetch active trips
        var activeTrips = await unitOfWork.Trips.Query()
            .Where(trip=>trip.DeletedById == null)
            .Where(trip => trip.EndTime >= DateTimeHelper.VnNow().Date)
            .ToListAsync(stoppingToken);

        // Update weather for each trip
        foreach (var trip in activeTrips)
        {
            var (forecasts, alerts) = await WeatherUpdateHelper.GetTripWeather(
                trip.StartTime,
                trip.Id,
                unitOfWork);

            Console.WriteLine(JsonConvert.SerializeObject(forecasts));
            Console.WriteLine(JsonConvert.SerializeObject(alerts));

            unitOfWork.WeatherAlerts.RemoveRange(
                unitOfWork.WeatherAlerts.Query().Where(e => e.TripId == trip.Id));

            var forecastDayMin = forecasts.Min(e => e.Date);
            unitOfWork.WeatherForecasts.RemoveRange(
                unitOfWork.WeatherForecasts.Query().Where(
                    e => e.TripId == trip.Id && e.Date >= forecastDayMin));

            unitOfWork.WeatherAlerts.AddRange(alerts);
            unitOfWork.WeatherForecasts.AddRange(forecasts);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
using Data.EFCore;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Service.Channels.Notification;
using Service.Implementations;
using Shared.Helpers;

namespace Application.Workers.Weather;

public class UpdateWeatherWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<UpdateWeatherWorker> _logger;

    public UpdateWeatherWorker(IServiceProvider services, ILogger<UpdateWeatherWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpdateWeatherWorker Started!");

        // Create and start Scheduler
        var schedulerFactory = new StdSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        scheduler.JobFactory = new WeatherUpdateJobFactory(_services);
        await scheduler.Start(stoppingToken);

        // Create a job
        var job = JobBuilder.Create<WeatherUpdateJob>()
            .WithIdentity("UpdateWeatherJob", "Jobs")
            .Build();

        // Create a trigger
        var trigger = TriggerBuilder.Create()
            .WithIdentity("UpdateWeatherTrigger", "Triggers")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(6)
                .RepeatForever())
            .Build();

        // Schedule the job with the trigger
        await scheduler.ScheduleJob(job, trigger, stoppingToken);
    }
}

// Define job
public class WeatherUpdateJob : IJob
{
    private readonly IServiceProvider _services;

    public WeatherUpdateJob(IServiceProvider services)
    {
        _services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _updateWeatherAlerts();
    }

    private async Task _updateWeatherAlerts()
    {
        // Get dependencies
        using var scope = _services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WeatherUpdateJob>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        var weatherDataFetcher = scope.ServiceProvider.GetRequiredService<WeatherDataFetcher>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        logger.LogInformation("Executing weather update job at {Time}", DateTime.Now);

        // Fetch active trips
        var activeTrips = await unitOfWork.Trips.Query()
            .Where(trip => trip.DeletedById == null &&
                           trip.Tour.DeletedById == null)
            .Where(trip => trip.EndTime >= DateTimeHelper.VnNow().Date)
            .ToListAsync();

        // Update weather for each trip
        foreach (var trip in activeTrips)
        {
            // Fetch weather data
            var alerts = await weatherDataFetcher.GetTripWeather(
                trip.StartTime, trip.Id, unitOfWork);

            // Remove old records
            unitOfWork.WeatherAlerts.RemoveRange(
                unitOfWork.WeatherAlerts.Query().Where(e => e.TripId == trip.Id));
            unitOfWork.WeatherAlerts.AddRange(alerts);
            await unitOfWork.SaveChangesAsync();

            // Send notification
            var groups = await unitOfWork.TourGroups.Query()
                .Where(g => g.TripId == trip.Id)
                .Select(g => new { g.TourGuideId, TravelerIds = g.Travelers.Select(t => t.Id) })
                .ToListAsync();

            var receiverIds = groups.Select(x =>
                {
                    var ids = x.TravelerIds.ToList();
                    if (x.TourGuideId != null) ids.Add(x.TourGuideId.Value);
                    return ids;
                })
                .SelectMany(x => x).ToList();

            foreach (var alert in alerts)
            {
                await notificationService.EnqueueNotification(new NotificationJob(
                    trip.Id, receiverIds, NotificationType.WeatherAlert, null,
                    alert.Event, alert.Headline));
            }
        }

        logger.LogInformation("Finish weather update job at {Time}", DateTime.Now);
    }
}

// For injecting services
public class WeatherUpdateJobFactory : IJobFactory
{
    private readonly IServiceProvider _services;

    public WeatherUpdateJobFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        => new WeatherUpdateJob(_services);

    public void ReturnJob(IJob job)
    {
        // do nothing
    }
}
using Data.EFCore;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Shared.Helpers;

namespace Application.Workers;

public class WeatherForecastWorker : BackgroundService
{
    private const string ApiKey = "e9a560f9a61e445193792558231607";
    private const int ForecastDays = 3;
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public WeatherForecastWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _client = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UpdateAlerts();

        var schedulerFactory = new StdSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

        await scheduler.Start(stoppingToken);

        // Define the job and tie it to our SimpleJob class
        var job = JobBuilder.Create<UpdateAlertsJob>()
            .WithIdentity("update_alerts_job", "group_1")
            .Build();

        // Define the trigger to run daily at 00:00
        var trigger = TriggerBuilder.Create()
            .WithIdentity("trigger_1", "group_1")
            .WithCronSchedule("0 0 0 * * ?") // This cron expression runs at 00:00 daily
            .Build();

        // Schedule the job with the trigger
        await scheduler.ScheduleJob(job, trigger, stoppingToken);
    }

    private class UpdateAlertsJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
        }
    }

    private async Task UpdateAlerts()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        var availableTrips = await unitOfWork.Trips
            .Query()
            .Where(trip =>
                DateTimeHelper.VnNow() >= trip.StartTime.AddDays(-3) &&
                DateTimeHelper.VnNow() <= trip.EndTime
            )
            .Include(trip =>
                trip.Tour.Schedules.Where(
                    schedule => schedule.Longitude != null && schedule.Latitude != null)
            )
            .AsSplitQuery()
            .ToListAsync();

        Console.WriteLine("Weather Forecast started.");
        Console.WriteLine($"Available Trips: {availableTrips.Count}");

        availableTrips = availableTrips
            .Select(trip =>
            {
                var alerts = GetWeatherAlerts(trip.StartTime, trip.Tour.Schedules).ToList();
                trip.WeatherAlerts = alerts;
                return trip;
            }).ToList();

        // Save alerts to DB
        foreach (var trip in availableTrips)
        {
            unitOfWork.WeatherAlerts.RemoveRange(
                unitOfWork.WeatherAlerts
                    .Query()
                    .Where(e => e.TripId == trip.Id)
            );

            unitOfWork.WeatherAlerts.AddRange(trip.WeatherAlerts.Select(e =>
            {
                e.TripId = trip.Id;
                return e;
            }));
        }

        await unitOfWork.SaveChangesAsync();
    }

    private IEnumerable<WeatherAlert> GetWeatherAlerts(
        DateTime startDate, IEnumerable<Schedule> schedules)
    {
        // Group schedule by DayNo
        var scheduleGroups = schedules.GroupBy(schedule => schedule.DayNo);

        var alertModels = scheduleGroups.Select(scheduleGroup =>
            {
                Console.WriteLine($"Day No: {scheduleGroup.Key}");

                // All forecast models for locations
                var forecastModels = scheduleGroup.Select(schedule =>
                {
                    var response = _client.GetAsync(
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

                    return model;
                });

                // Filter out duplicate area
                var models = forecastModels
                    .GroupBy(model => model.Location.Name)
                    .Select(grouping => grouping.First());

                var startSelectedDate = startDate.AddDays(scheduleGroup.Key - 1);
                var endSelectedDate = startSelectedDate.AddDays(1);

                // filter out
                var alerts = models
                    .SelectMany(model => model.Alerts.Alert)
                    .Where(alert => !(
                        (alert.Effective.ToVnNow() < startSelectedDate &&
                         alert.Expires.ToVnNow() < startSelectedDate)
                        ||
                        (alert.Effective.ToVnNow() > endSelectedDate
                         && alert.Expires.ToVnNow() > endSelectedDate))
                    )
                    .ToList();

                return alerts;
            }
        ).SelectMany(e => e);

        return alertModels.Select(model => new WeatherAlert()
        {
            Areas = model.Areas,
            Certainty = model.Certainty,
            Effective = model.Effective.DateTime,
            Expires = model.Expires.DateTime,
            Event = model.Event,
            Headline = model.Headline,
            Instruction = model.Instruction,
            Note = model.Note,
            Severity = model.Severity,
            Description = model.Desc,
        }).ToList();
    }

    public class WeatherForecastModel
    {
        public Location Location { get; set; } = null!;
        public Alerts Alerts { get; set; } = null!;
    }

    public class Location
    {
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class Alerts
    {
        public List<Alert> Alert { get; set; } = new();
    }

    public class Alert
    {
        public string Headline { get; set; } = null!;
        public string Msgtype { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public string Urgency { get; set; } = null!;
        public string Areas { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Certainty { get; set; } = null!;
        public string Event { get; set; } = null!;
        public string Note { get; set; } = null!;
        public DateTimeOffset Effective { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string Desc { get; set; } = null!;
        public string Instruction { get; set; } = null!;
    }
}
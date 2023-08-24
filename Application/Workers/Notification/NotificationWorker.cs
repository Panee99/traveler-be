using Data.EFCore;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using RazorEngineCore;
using Service.Channels.Notification;
using Service.Interfaces;

namespace Application.Workers.Notification;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly ICloudNotificationService _cloudNotificationService;
    private readonly INotificationJobQueue _jobQueue;
    private readonly IServiceProvider _serviceProvider;

    private readonly IRazorEngineCompiledTemplate _attendanceTemplate;
    private readonly IRazorEngineCompiledTemplate _emergencyTemplate;
    private readonly IRazorEngineCompiledTemplate _weatherAlertTemplate;

    public NotificationWorker(
        INotificationJobQueue jobQueue,
        ICloudNotificationService cloudNotificationService,
        ILogger<NotificationWorker> logger,
        IServiceProvider serviceProvider)
    {
        _jobQueue = jobQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _cloudNotificationService = cloudNotificationService;
        //
        var razorEngine = new RazorEngine();
        _attendanceTemplate = razorEngine.Compile(_readTemplate("attendance.html"));
        _emergencyTemplate = razorEngine.Compile(_readTemplate("emergency.html"));
        _weatherAlertTemplate = razorEngine.Compile(_readTemplate("weather-alert.html"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification worker started. Queue capacity: {Capacity}", _jobQueue.GetCapacity());
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _jobQueue.DequeueAsync();
                _ = Task.Run(
                    async () =>
                    {
                        try
                        {
                            await _handleNotification(job);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing notification task: {Message}", ex.Message);
            }
        }
    }

    // Handler worker stop event
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(NotificationWorker)} is stopping.");
        await base.StopAsync(stoppingToken);
    }

    // Handle Notification by types
    private async Task _handleNotification(NotificationJob job)
    {
        // Get Dependencies
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Get fcm tokens
        var fcmTokens = await unitOfWork.FcmTokens
            .Query()
            .Where(e => job.ReceiverIds.Contains(e.UserId))
            .Select(e => e.Token)
            .ToListAsync();

        var title = _buildTitle(job.Type);
        switch (job.Type)
        {
            case NotificationType.AttendanceActivity:
                await notificationService.SaveNotifications(job.TripId, job.ReceiverIds, job.Type, title,
                    await _attendanceTemplate.RunAsync(), job.ImageId);

                await _cloudNotificationService.SendBatchMessages(fcmTokens, job.Type, title,
                    $"A new attendance activity opened. {job.Data[0]}");
                break;

            case NotificationType.Emergency:
                await notificationService.SaveNotifications(job.TripId, job.ReceiverIds, job.Type, title,
                    await _emergencyTemplate.RunAsync(new { Name = job.Data[0] }), job.ImageId);

                await _cloudNotificationService.SendBatchMessages(fcmTokens, job.Type, title,
                    $"Notice! User {job.Data[0]} sent emergency request.");
                break;

            case NotificationType.WeatherAlert:
                await notificationService.SaveNotifications(job.TripId, job.ReceiverIds, job.Type, title,
                    await _weatherAlertTemplate.RunAsync(
                        new { Event = job.Data[0], Headline = job.Data[1] }),
                    job.ImageId);

                await _cloudNotificationService.SendBatchMessages(fcmTokens, job.Type, title,
                    $"{job.Data[0]}! {job.Data[1]}.");
                break;

            // case NotificationType.CheckInActivity:
            //     break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Create string title from NotificationType
    private string _buildTitle(NotificationType type)
    {
        return type switch
        {
            NotificationType.AttendanceActivity => "Attendance Activity",
            NotificationType.Emergency => "Emergency",
            NotificationType.WeatherAlert => "Weather Alert!",
            // NotificationType.CheckInActivity => "Check In Activity",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // Read .html template file
    private static string _readTemplate(string fileName)
    {
        return File.ReadAllText($"Workers/Notification/Templates/{fileName}");
    }
}
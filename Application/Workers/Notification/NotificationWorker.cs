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
                    async () => { await _handleNotification(job); }, stoppingToken);
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
                await notificationService.SaveNotifications(job.ReceiverIds, title,
                    await _attendanceTemplate.RunAsync(new { Name = job.Subject }), job.Type, job.ImageId);

                await _cloudNotificationService.SendBatchMessages(fcmTokens, title,
                    $"A new attendance activity opened. {job.Subject}", job.Type);
                break;

            case NotificationType.Emergency:
                await notificationService.SaveNotifications(job.ReceiverIds, title,
                    await _emergencyTemplate.RunAsync(new { Name = job.Subject }), job.Type, job.ImageId);

                await _cloudNotificationService.SendBatchMessages(fcmTokens, title,
                    $"Notice! User {job.Subject} sent emergency request.", job.Type);
                break;

            case NotificationType.WeatherAlert:
                await _cloudNotificationService.SendBatchMessages(fcmTokens, title, job.Subject, job.Type);
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
            // NotificationType.CheckInActivity => "Check In Activity",
            NotificationType.WeatherAlert => "Weather Alert!",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // Read .html template file
    private static string _readTemplate(string fileName)
    {
        return File.ReadAllText($"Workers/Notification/Templates/{fileName}");
    }
}
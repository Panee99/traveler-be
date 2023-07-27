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

    private readonly RazorEngine _razorEngine;
    private readonly string _attendanceTemplate;

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
        _razorEngine = new RazorEngine();
        _attendanceTemplate = _readTemplate("attendance.html");
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
                        using var scope = _serviceProvider.CreateScope();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var title = _buildTitle(job.Type);
                        var rawPayload = _buildPayload(job.Type, job.Subject, job.DirectObject, false);
                        var templatePayload = _buildPayload(job.Type, job.Subject, job.DirectObject, true);

                        // 1. Save notifications
                        await notificationService.SaveNotifications(job.ReceiverIds,
                            title,
                            templatePayload,
                            job.Type);

                        // 2. Send notifications
                        var fcmTokens = await unitOfWork.FcmTokens
                            .Query()
                            .Where(e => job.ReceiverIds.Contains(e.UserId))
                            .Select(e => e.Token)
                            .ToListAsync(stoppingToken);

                        await _cloudNotificationService.SendBatchMessages(
                            fcmTokens, title, rawPayload, job.Type);
                    },
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing notification task: {Message}", ex.Message);
            }
            // catch (OperationCanceledException) {}
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(NotificationWorker)} is stopping.");
        await base.StopAsync(stoppingToken);
    }

    private string _buildTitle(NotificationType type)
    {
        return type switch
        {
            NotificationType.AttendanceActivity => "Attendance Activity",
            NotificationType.TourStarted => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string _buildPayload(NotificationType type, string subject, string directObject, bool useTemplate)
    {
        switch (type)
        {
            case NotificationType.AttendanceActivity:
            {
                if (!useTemplate)
                {
                    return $"A new attendance activity opened: {directObject}";
                }

                var compiledTemplate = _razorEngine.Compile(_attendanceTemplate);
                return compiledTemplate.Run(new { Name = directObject });
            }
            case NotificationType.TourStarted: throw new ArgumentOutOfRangeException();
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private static string _readTemplate(string fileName)
    {
        return File.ReadAllText($"Workers/Notification/Templates/{fileName}");
    }
}
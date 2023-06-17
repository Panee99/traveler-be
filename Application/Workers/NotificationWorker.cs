using Data.EFCore;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Interfaces;
using Shared.Helpers;

namespace Application.Workers;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly ICloudNotificationService _cloudNotificationService;
    private readonly INotificationJobQueue _jobQueue;
    private readonly IServiceProvider _serviceProvider;

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

                        // 1. Save notifications
                        var notifications = job.ReceiverIds.Select(travelerId => new Notification()
                        {
                            ReceiverId = travelerId,
                            Title = job.Title,
                            Payload = job.Payload,
                            Timestamp = DateTimeHelper.VnNow(),
                            Type = job.Type
                        });

                        unitOfWork.Notifications.AddRange(notifications);
                        await unitOfWork.SaveChangesAsync();

                        // 2. Send notifications
                        var fcmTokens = await unitOfWork.FcmTokens
                            .Query()
                            .Where(e => job.ReceiverIds.Contains(e.UserId))
                            .Select(e => e.Token)
                            .ToListAsync(stoppingToken);

                        await _cloudNotificationService.SendBatchMessages(
                            fcmTokens, job.Title, job.Payload, job.Type.ToString());
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
}
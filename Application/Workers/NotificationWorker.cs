using Newtonsoft.Json;
using Service.Interfaces;
using Shared.Channels.Notification;

namespace Application.Workers;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly ICloudNotificationService _cloudNotificationService;
    private readonly INotificationJobQueue _jobQueue;

    public NotificationWorker(
        INotificationJobQueue jobQueue,
        ICloudNotificationService cloudNotificationService,
        ILogger<NotificationWorker> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
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
                //
                _cloudNotificationService.SendBatchMessages(job.ReceiverTokens, job.Title, job.Payload);
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
namespace Service.Channels.Notification;

public interface INotificationJobQueue
{
    ValueTask EnqueueAsync(NotificationJob job);

    ValueTask<NotificationJob> DequeueAsync();

    int GetCapacity();
}
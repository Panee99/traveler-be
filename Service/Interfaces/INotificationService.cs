using Data.Enums;
using Service.Channels.Notification;
using Service.Models.Notification;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface INotificationService
{
    Task<Result<List<NotificationViewModel>>> ListAll(Guid userId, Guid tripId);

    Task SaveNotifications(Guid? tripId, IEnumerable<Guid> receiverIds, NotificationType type,
        string title, string payload, Guid? imageId);

    Task<Result> MarkAsRead(Guid notificationId);

    Task<Result> MarkAllAsRead(Guid userId);

    Task<Result<int>> GetUnreadCount(Guid userId);

    Task EnqueueNotification(NotificationJob notificationJob);
}
using Data.Enums;
using Service.Models.Notification;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface INotificationService
{
    Task<Result<List<NotificationViewModel>>> ListAll(Guid userId);

    Task AddNotifications(IEnumerable<Guid> receiverIds, string title, string payload, NotificationType type);

    Task<Result> MarkAsRead(Guid notificationId);

    Task<Result> MarkAllAsRead(Guid userId);

    Task<Result<int>> GetUnreadCount(Guid userId);
}
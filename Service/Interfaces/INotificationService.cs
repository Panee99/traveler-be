using Service.Models.Notification;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface INotificationService
{
    Task<Result<List<NotificationViewModel>>> ListAll(Guid userId);
}
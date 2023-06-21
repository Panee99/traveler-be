using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Notification;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class NotificationService : BaseService, INotificationService
{
    public NotificationService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<List<NotificationViewModel>>> ListAll(Guid userId)
    {
        return await UnitOfWork.Notifications
            .Query()
            .Where(e => e.ReceiverId == userId)
            .ProjectToType<NotificationViewModel>()
            .ToListAsync();
    }

    public async Task<Result> MarkAsRead(Guid notificationId)
    {
        var notification = await UnitOfWork.Notifications.FindAsync(notificationId);
        if (notification is null) return Error.NotFound();

        notification.IsRead = true;
        UnitOfWork.Notifications.Update(notification);
        await UnitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result> MarkAllAsRead()
    {
        var notifications = await UnitOfWork.Notifications
            .Query()
            .Where(e => e.IsRead == false)
            .ToListAsync();

        notifications = notifications.Select(e =>
        {
            e.IsRead = true;
            return e;
        }).ToList();

        UnitOfWork.Notifications.UpdateRange(notifications);
        await UnitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task AddNotifications(
        IEnumerable<Guid> receiverIds,
        string title,
        string payload,
        NotificationType type)
    {
        var notifications = receiverIds.Select(travelerId => new Notification()
        {
            ReceiverId = travelerId,
            Title = title,
            Payload = payload,
            Timestamp = DateTimeHelper.VnNow(),
            IsRead = false,
            Type = type
        });

        UnitOfWork.Notifications.AddRange(notifications);
        await UnitOfWork.SaveChangesAsync();
    }
}
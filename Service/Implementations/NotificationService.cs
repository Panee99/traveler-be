using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
using Service.Interfaces;
using Service.Models.Notification;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class NotificationService : BaseService, INotificationService
{
    private readonly ICloudStorageService _cloudStorageService;

    public NotificationService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<List<NotificationViewModel>>> ListAll(Guid userId)
    {
        var notifications = await UnitOfWork.Notifications
            .Query()
            .Where(e => e.ReceiverId == userId)
            .ToListAsync();

        return notifications.Select(notification =>
        {
            var view = notification.Adapt<NotificationViewModel>();
            view.ImageUrl = _cloudStorageService.GetMediaLink(notification.ImageId)!;
            return view;
        }).ToList();
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

    public async Task<Result> MarkAllAsRead(Guid userId)
    {
        var notifications = await UnitOfWork.Notifications
            .Query()
            .Where(e => e.ReceiverId == userId)
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

    public async Task<Result<int>> GetUnreadCount(Guid userId)
    {
        return await UnitOfWork.Notifications
            .Query()
            .Where(e => e.ReceiverId == userId)
            .Where(e => e.IsRead == false)
            .CountAsync();
    }

    public async Task AddNotifications(
        IEnumerable<Guid> receiverIds,
        string title,
        string payload,
        NotificationType type)
    {
        var notifications = receiverIds.Select(travelerId =>
        {
            return new Notification()
            {
                ReceiverId = travelerId,
                Title = title,
                Payload = payload,
                Timestamp = DateTimeHelper.VnNow(),
                IsRead = false,
                Type = type,
                ImageId = type switch
                {
                    NotificationType.AttendanceEvent => ServiceConstants.AttendanceImage,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        });

        UnitOfWork.Notifications.AddRange(notifications);
        await UnitOfWork.SaveChangesAsync();
    }
}
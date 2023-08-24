using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Channels.Notification;
using Service.Interfaces;
using Service.Models.Notification;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class NotificationService : BaseService, INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly INotificationJobQueue _notificationJobQueue;

    public NotificationService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService,
        INotificationJobQueue notificationJobQueue, ILogger<NotificationService> logger) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _notificationJobQueue = notificationJobQueue;
        _logger = logger;
    }

    public async Task EnqueueNotification(NotificationJob notificationJob)
    {
        await _notificationJobQueue.EnqueueAsync(notificationJob);
    }

    public async Task<Result<List<NotificationViewModel>>> ListAll(Guid userId)
    {
        var notifications = await UnitOfWork.Notifications
            .Query()
            .Where(e => e.ReceiverId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Include(e => e.Image)
            .ToListAsync();

        return notifications.Select(notification =>
        {
            var view = notification.Adapt<NotificationViewModel>();
            view.ImageUrl = _cloudStorageService.GetMediaLink(notification.Image?.FileName)!;
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

    public async Task SaveNotifications(
        Guid? tripId,
        IEnumerable<Guid> receiverIds,
        NotificationType type,
        string title,
        string payload,
        Guid? imageId)
    {
        var notifications = receiverIds.Select(travelerId => new Notification()
        {
            TripId = tripId,
            ReceiverId = travelerId,
            Title = title,
            Payload = payload,
            Timestamp = DateTimeHelper.VnNow(),
            IsRead = false,
            Type = type,
            ImageId = imageId
        });

        try
        {
            UnitOfWork.Notifications.AddRange(notifications);
            await UnitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Saving notification failed");
        }
    }
}
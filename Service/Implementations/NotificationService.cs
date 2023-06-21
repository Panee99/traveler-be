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
            Type = type
        });

        UnitOfWork.Notifications.AddRange(notifications);
        await UnitOfWork.SaveChangesAsync();
    }
}
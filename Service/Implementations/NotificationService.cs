using Data.EFCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Notification;
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
}
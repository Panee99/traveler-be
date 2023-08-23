using Data.Enums;

namespace Service.Interfaces;

public interface ICloudNotificationService
{
    Task SendBatchMessages(ICollection<string> tokens, NotificationType type, string title, string payload);
}
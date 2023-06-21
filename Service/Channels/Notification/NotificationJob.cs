using Data.Enums;

namespace Service.Channels.Notification;

public record NotificationJob
(
    ICollection<Guid> ReceiverIds,
    string Subject,
    string DirectObject,
    NotificationType Type
);
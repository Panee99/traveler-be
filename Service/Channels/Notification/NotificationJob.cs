using Data.Enums;

namespace Service.Channels.Notification;

public record NotificationJob
(
    ICollection<Guid> ReceiverIds,
    NotificationType Type,
    string Subject,
    string? DirectObject,
    Guid? ImageId
);

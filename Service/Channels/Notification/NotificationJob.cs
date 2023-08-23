using Data.Enums;

namespace Service.Channels.Notification;

public record NotificationJob
(
    ICollection<Guid> ReceiverIds,
    NotificationType Type,
    Guid? ImageId,
    params string[] Data
);
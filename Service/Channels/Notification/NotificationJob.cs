using Data.Enums;

namespace Service.Channels.Notification;

public record NotificationJob
(
    ICollection<Guid> ReceiverIds,
    string Title,
    string Payload,
    NotificationType Type
);
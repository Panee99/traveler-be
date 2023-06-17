namespace Shared.Channels.Notification;

public record NotificationJob
(
    ICollection<string> ReceiverTokens,
    string Title,
    string Payload
);
using Data.Enums;

namespace Data.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid ReceiverId { get; set; }

    public User Receiver { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime Timestamp { get; set; }

    public Guid ImageId { get; set; }

    public Attachment Image { get; set; } = null!;
}
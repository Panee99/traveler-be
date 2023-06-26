using Data.Enums;

namespace Service.Models.Notification;

public class NotificationViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
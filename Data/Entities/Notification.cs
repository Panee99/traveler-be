namespace Data.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid ReceiverId { get; set; }

    public User Receiver { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
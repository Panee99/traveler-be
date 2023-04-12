using Data.Enums;

namespace Data.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public TicketType Type { get; set; }
    public Guid TourId { get; set; }
    public Guid TravelerId { get; set; }
    public Guid? ImageId { get; set; }

    public Tour Tour { get; set; } = null!;
    public Traveler Traveler { get; set; } = null!;
    public Attachment? Image { get; set; } = null!;
}
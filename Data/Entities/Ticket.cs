using Data.Enums;

namespace Data.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public TicketType Type { get; set; }
    public Guid TourId { get; set; }
    public Guid TravelerId { get; set; }
    public Guid? AttachmentId { get; set; }
    
    public Tour Tour { get; set; } = null!;
    public Traveler Traveler { get; set; } = null!;
    public Attachment? Attachment { get; set; } = null!;
}


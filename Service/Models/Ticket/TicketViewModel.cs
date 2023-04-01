using Data.Enums;

namespace Service.Models.Ticket;

public record TicketViewModel
{
    public Guid Id;
    public string Content = null!;
    public TicketType Type;
    public Guid TourId;
    public Guid TravelerId;
    public string? ImageUrl;
}
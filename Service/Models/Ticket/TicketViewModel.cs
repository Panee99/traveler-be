using Data.Enums;

namespace Service.Models.Ticket;

public record TicketViewModel
{
    public string Content = null!;
    public Guid Id;
    public string? ImageUrl;
    public Guid TourId;
    public Guid TravelerId;
    public TicketType Type;
}
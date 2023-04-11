namespace Service.Models.Ticket;

public record TicketFilterModel
{
    public Guid? TourId;
    public Guid? TravelerId;
}
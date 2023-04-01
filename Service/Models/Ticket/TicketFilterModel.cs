namespace Service.Models.Ticket;

public record TicketFilterModel : PagingFilterModel
{
    public Guid? TourId;
    public Guid? TravelerId;
}
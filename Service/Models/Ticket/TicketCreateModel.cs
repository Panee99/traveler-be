using Data.Enums;

namespace Service.Models.Ticket;

public record TicketCreateModel(
    string Content,
    TicketType Type,
    Guid TourId,
    Guid TravelerId,
    Guid? ImageId
);
using Service.Models.Ticket;
using Service.Pagination;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITicketService
{
    Task<Result<TicketViewModel>> Create(TicketCreateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TicketViewModel>> Find(Guid id);

    Task<Result<PaginationModel<TicketViewModel>>> Filter(TicketFilterModel model);

    Task<Result> UpdateImage(Guid ticketId, string contentType, Stream stream);
}
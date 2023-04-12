using Service.Models.Attachment;
using Service.Models.Ticket;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITicketService
{
    Task<Result<TicketViewModel>> Create(TicketCreateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TicketViewModel>> Find(Guid id);

    Task<Result<List<TicketViewModel>>> Filter(TicketFilterModel model);

    Task<Result<AttachmentViewModel>> UpdateImage(Guid ticketId, string contentType, Stream stream);
}
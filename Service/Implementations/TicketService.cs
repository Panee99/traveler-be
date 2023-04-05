using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Ticket;
using Service.Pagination;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TicketService : BaseService, ITicketService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService,
        ILogger<TicketService> logger) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _logger = logger;
    }

    public async Task<Result<TicketViewModel>> Create(TicketCreateModel model)
    {
        var entity = UnitOfWork.Repo<Ticket>().Add(model.Adapt<Ticket>());

        await UnitOfWork.SaveChangesAsync();

        return entity.Adapt<TicketViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        UnitOfWork.Repo<Ticket>().Remove(new Ticket { Id = id });

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TicketViewModel>> Find(Guid id)
    {
        var ticket = await UnitOfWork.Repo<Ticket>().FirstOrDefaultAsync(e => e.Id == id);

        if (ticket is null) return Error.NotFound();

        var view = ticket.Adapt<TicketViewModel>();

        if (ticket.ImageId != null)
            view.ImageUrl = _cloudStorageService.GetMediaLink(ticket.ImageId.Value);

        return view;
    }

    public async Task<Result<PaginationModel<TicketViewModel>>> Filter(TicketFilterModel model)
    {
        var query = UnitOfWork.Repo<Ticket>().Query();

        if (model.TourId != null) query = query.Where(e => e.TourId == model.TourId);

        if (model.TravelerId != null) query = query.Where(e => e.TravelerId == model.TravelerId);

        var paginationModel = await query.Paging(model.Page, model.Size);

        return paginationModel.Map(e =>
        {
            var view = e.Adapt<TicketViewModel>();
            if (e.ImageId != null) view.ImageUrl = _cloudStorageService.GetMediaLink(e.ImageId.Value);
            return view;
        });
    }

    public async Task<Result> UpdateImage(Guid ticketId, string contentType, Stream stream)
    {
        // Get Ticket and old ImageId
        var ticket = await UnitOfWork.Repo<Ticket>().Query().Where(e => e.Id == ticketId)
            .Select(e => new Ticket { Id = ticketId, ImageId = e.ImageId }).FirstOrDefaultAsync();

        var oldImage = ticket?.ImageId;

        if (ticket is null) return Error.NotFound();

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Create new Attachment
            ticket.Image = new Attachment
            {
                ContentType = contentType
            };

            UnitOfWork.Attach(ticket);
            UnitOfWork.Entry(ticket.Image).State = EntityState.Added;

            await UnitOfWork.SaveChangesAsync();

            // Upload to Cloud
            var uploadResult = await _cloudStorageService.Upload(ticket.Image.Id, contentType, stream);
            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            // Remove old Image
            if (oldImage != null)
            {
                UnitOfWork.Repo<Attachment>().Remove(new Attachment { Id = oldImage.Value });
                await UnitOfWork.SaveChangesAsync();

                var deleteResult = await _cloudStorageService.Delete(oldImage.Value);
                if (!deleteResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Error.Unexpected();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }

        await transaction.CommitAsync();
        return Result.Success();
    }
}
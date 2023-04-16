using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Ticket;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TicketService : BaseService, ITicketService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<TicketService> _logger;

    //
    private readonly IRepository<Ticket> _ticketRepo;
    private readonly IRepository<Ticket> _tourRepo;
    private readonly IRepository<Ticket> _travelerRepo;

    public TicketService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService,
        ILogger<TicketService> logger, IAttachmentService attachmentService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _attachmentService = attachmentService;
        _logger = logger;
        //
        _ticketRepo = unitOfWork.Repo<Ticket>();
        _tourRepo = unitOfWork.Repo<Ticket>();
        _travelerRepo = unitOfWork.Repo<Ticket>();
    }

    public async Task<Result<TicketViewModel>> Create(TicketCreateModel model)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == model.TourId))
            return Error.NotFound("Tour not found.");

        if (!await _travelerRepo.AnyAsync(e => e.Id == model.TravelerId))
            return Error.NotFound("Traveler not found.");

        var entity = _ticketRepo.Add(model.Adapt<Ticket>());

        await UnitOfWork.SaveChangesAsync();

        return entity.Adapt<TicketViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        _ticketRepo.Remove(new Ticket { Id = id });

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TicketViewModel>> Find(Guid id)
    {
        var ticket = await _ticketRepo.FindAsync(id);

        if (ticket is null) return Error.NotFound();

        var view = ticket.Adapt<TicketViewModel>();

        if (ticket.ImageId != null)
            view.ImageUrl = _cloudStorageService.GetMediaLink(ticket.ImageId.Value);

        return view;
    }

    public async Task<Result<List<TicketViewModel>>> Filter(TicketFilterModel model)
    {
        var query = _ticketRepo.Query();

        if (model.TourId != null) query = query.Where(e => e.TourId == model.TourId);
        if (model.TravelerId != null) query = query.Where(e => e.TravelerId == model.TravelerId);

        var tickets = await query.ToListAsync();

        return tickets.Select(e =>
        {
            var view = e.Adapt<TicketViewModel>();
            if (e.ImageId != null) view.ImageUrl = _cloudStorageService.GetMediaLink(e.ImageId.Value);
            return view;
        }).ToList();
    }

    public async Task<Result<AttachmentViewModel>> UpdateImage(Guid ticketId, string contentType, Stream stream)
    {
        // Get Ticket and old ImageId
        var ticket = await _ticketRepo
            .Query()
            .Where(e => e.Id == ticketId)
            .Select(e => new Ticket { Id = ticketId, ImageId = e.ImageId })
            .FirstOrDefaultAsync();

        if (ticket is null) return Error.NotFound();

        var oldImageId = ticket.ImageId;

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Create new Attachment
            var createAttachmentResult = await _attachmentService.Create(contentType, stream);

            if (!createAttachmentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            UnitOfWork.Attach(ticket);
            ticket.ImageId = createAttachmentResult.Value.Id;

            await UnitOfWork.SaveChangesAsync();

            // Delete old Image
            if (oldImageId != null)
            {
                var deleteAttachmentResult = await _attachmentService.Delete(oldImageId.Value);
                if (!deleteAttachmentResult.IsSuccess)
                    _logger.LogError("Delete attachment failed: {Id}", oldImageId.Value);
            }

            await transaction.CommitAsync();
            return createAttachmentResult.Value;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            await transaction.RollbackAsync();
            return Error.Unexpected(e.Message);
        }
    }
}
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Shared.ExternalServices.VnPay.Models;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class VnPayService : BaseService, IVnPayService
{
    private readonly ILogger<VnPayService> _logger;
    private readonly IMapper _mapper;

    public VnPayService(UnitOfWork unitOfWork, IMapper mapper, ILogger<VnPayService> logger) : base(unitOfWork)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Guid>> CreateRequest(VnPayRequestModel model)
    {
        var entity = _mapper.Map<VnPayRequest>(model);
        UnitOfWork.VnPayRequests.Add(entity);
        await UnitOfWork.SaveChangesAsync();
        return entity.TxnRef;
    }

    public async Task<Result> HandleResponse(VnPayResponseModel model)
    {
        var request = await UnitOfWork.VnPayRequests
            .Query()
            .Where(e => e.TxnRef == model.TxnRef)
            .Include(e => e.Transaction)
            .ThenInclude(e => e.Booking)
            .FirstOrDefaultAsync();

        if (request is null)
        {
            _logger.LogError("Invalid response. Can not find VnPay TxnRef: {TxnRef}", model.TxnRef);
            return Error.Unexpected();
        }

        // Save Response
        var response = _mapper.Map<VnPayResponse>(model);
        response.Timestamp = DateTimeHelper.VnNow();
        UnitOfWork.VnPayResponses.Add(response);

        // Update Request and Transaction status
        if (IsSuccessResponse(model))
        {
            request.Status = VnPayRequestStatus.Success;
            request.Transaction.Status = TransactionStatus.Success;
            request.Transaction.Booking.PaymentStatus = PaymentStatus.Paid;
            
            // Assign traveler to a group
            // 1. find available group
            var booking = request.Transaction.Booking;
            var occupancy = TotalBookingOccupancy(booking);
            var availableTourGroup = await UnitOfWork.TourGroups
                .Query()
                .Where(tourGroup => tourGroup.TourId == booking.TourId)
                .Where(tourGroup => tourGroup.MaxOccupancy > tourGroup.TravelerInTours.Count + occupancy)
                .FirstOrDefaultAsync();

            // 2. create new group if all groups full
            if (availableTourGroup is null)
            {
                var tourGroupCount = await UnitOfWork.Tours
                    .Query()
                    .Where(e => e.Id == booking.TourId)
                    .Select(e => e.TourGroups)
                    .CountAsync();

                availableTourGroup = new TourGroup()
                {
                    GroupName = $"Group {tourGroupCount + 1}",
                    TourId = booking.TourId,
                    MaxOccupancy = 50,
                };

                UnitOfWork.TourGroups.Add(availableTourGroup);
            }

            // 3. add traveler to group
            UnitOfWork.TravelersInTours.Add(new TravelerInTour()
            {
                TourId = booking.TourId,
                TravelerId = booking.TravelerId,
                TourGroupId = availableTourGroup.Id
            });
        }
        else
        {
            request.Status = VnPayRequestStatus.Failed;
            request.Transaction.Status = TransactionStatus.Failed;
        }

        UnitOfWork.VnPayRequests.Update(request);

        // Finalize
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    private static bool IsSuccessResponse(VnPayResponseModel model)
    {
        return model is { ResponseCode: "00", TransactionStatus: "00" };
    }

    private static int TotalBookingOccupancy(Booking booking)
    {
        return booking.AdultQuantity + booking.ChildrenQuantity + booking.InfantQuantity;
    }
}
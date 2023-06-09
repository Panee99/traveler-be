using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Transaction;
using Shared.ExternalServices.VnPay;
using Shared.ExternalServices.VnPay.Models;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TransactionService : BaseService, ITransactionService
{
    private readonly VnPay _vnPay;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(UnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor, VnPay vnPay, ILogger<TransactionService> logger) :
        base(unitOfWork, httpContextAccessor)
    {
        _vnPay = vnPay;
        _logger = logger;
    }

    /// <summary>
    /// Create payment transaction
    /// </summary>
    public async Task<Result<TransactionViewModel>> CreateTransaction(Guid bookingId, string clientIp)
    {
        // find booking
        var booking = await UnitOfWork.Bookings.FindAsync(bookingId);
        if (booking is null) return Error.NotFound("Booking not found");

        // check if booking expired
        if (booking.ExpireAt <= DateTimeHelper.VnNow()) return Error.Conflict("Booking expired");

        // check owning logic
        if (CurrentUser!.Id != booking.TravelerId) return Error.Conflict("Can only pay your own booking");

        // check if booking status
        if (booking.Status is not BookingStatus.Pending)
            return Error.Conflict("Booking status must be Pending");

        // remove old transactions of this booking
        var oldTransactions = await UnitOfWork.Transactions
            .Query()
            .Where(e => e.BookingId == bookingId)
            .ToListAsync();

        UnitOfWork.Transactions.RemoveRange(oldTransactions);

        // create new transaction with payment url
        var amount = _calculateAmount(booking);

        var transaction = new Transaction()
        {
            BookingId = bookingId,
            Amount = amount,
            Status = TransactionStatus.Pending,
            Timestamp = DateTimeHelper.VnNow(),
            ClientIp = clientIp
        };

        UnitOfWork.Transactions.Add(transaction);

        await UnitOfWork.SaveChangesAsync();

        var requestModel =
            _buildVnPayRequestModel(clientIp, amount, bookingId, booking.TravelerId, transaction.Id);

        // Result
        var view = transaction.Adapt<TransactionViewModel>();
        view.PayUrl = _vnPay.CreateRequestUrl(requestModel);
        return view;
    }

    /// <summary>
    /// Handle payment response from VnPay server
    /// </summary>
    public async Task<Result> HandleIpnResponse(VnPayResponseModel model)
    {
        // find target transaction of this response
        var transaction = await UnitOfWork.Transactions
            .Query()
            .AsSplitQuery()
            .Where(trans => trans.Id == model.TxnRef)
            .Include(trans => trans.Booking)
            .ThenInclude(booking => booking.TourVariant)
            .FirstOrDefaultAsync();

        if (transaction is null)
        {
            _logger.LogError("Invalid response. No Transaction have matching TxnRef: {TxnRef}", model.TxnRef);
            return Error.Unexpected();
        }

        // Save Response
        var response = model.Adapt<VnPayResponse>();
        response.Timestamp = DateTimeHelper.VnNow();
        response.TransactionId = model.TxnRef;
        UnitOfWork.VnPayResponses.Add(response);

        // Update Transaction status
        if (_isSuccessResponse(model))
        {
            transaction.Status = TransactionStatus.Success;
            transaction.Booking.Status = BookingStatus.Paid;

            // Assign traveler to a group
            // 1. find available group
            var booking = transaction.Booking;
            var occupancy = _totalBookingOccupancy(booking);
            var availableTourGroup = await UnitOfWork.TourGroups
                .Query()
                .Where(tourGroup => tourGroup.TourVariantId == booking.TourVariantId)
                .Where(tourGroup => tourGroup.MaxOccupancy > tourGroup.Travelers.Count + occupancy)
                .FirstOrDefaultAsync();

            // 2. create new group if all groups full
            if (availableTourGroup is null)
            {
                // get tour group count. Ex: Exist 4 groups -> create "Group 5". 
                var tourGroupCount = await UnitOfWork.TourVariants
                    .Query()
                    .Where(e => e.Id == booking.TourVariantId)
                    .Select(e => e.TourGroups)
                    .CountAsync();

                availableTourGroup = new TourGroup()
                {
                    GroupName = $"Group {tourGroupCount + 1}",
                    TourVariantId = booking.TourVariantId,
                    MaxOccupancy = 50,
                };

                UnitOfWork.TourGroups.Add(availableTourGroup);
            }

            // 3. add traveler to group
            UnitOfWork.TravelersInTourGroups.Add(new TravelerInTourGroup()
            {
                TravelerId = booking.TravelerId,
                TourGroupId = availableTourGroup.Id,
                JoinedAt = DateTimeHelper.VnNow()
            });
        }
        else
        {
            transaction.Status = TransactionStatus.Failed;
        }

        UnitOfWork.Transactions.Update(transaction);

        // Finalize
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    private static bool _isSuccessResponse(VnPayResponseModel model)
    {
        return model is { ResponseCode: "00", TransactionStatus: "00" };
    }

    private static int _totalBookingOccupancy(Booking booking)
    {
        return booking.AdultQuantity + booking.ChildrenQuantity + booking.InfantQuantity;
    }


    /// <summary>
    /// PRIVATE
    /// </summary>
    private static VnPayRequestModel _buildVnPayRequestModel(
        string clientIp, int amount, Guid bookingId, Guid travelerId, Guid transactionId)
    {
        var now = DateTimeHelper.VnNow();

        return new VnPayRequestModel()
        {
            TxnRef = transactionId,
            Amount = amount,
            CreateDate = now,
            ExpireDate = now.AddMinutes(15),
            OrderInfo = $"traveler '{travelerId}' pay tour booking '{bookingId}', amount = {amount} vnd",
            IpAddress = clientIp,
        };
    }

    private static int _calculateAmount(Booking booking)
    {
        return (booking.AdultPrice * booking.AdultQuantity) +
               (booking.ChildrenPrice * booking.ChildrenQuantity) +
               (booking.InfantPrice * booking.InfantQuantity);
    }
}
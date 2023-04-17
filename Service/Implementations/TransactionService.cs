using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Models.Transaction;
using Shared.ExternalServices.VnPay;
using Shared.ExternalServices.VnPay.Models;
using Shared.Helpers;
using Shared.ResultExtensions;
using Shared.Settings;

namespace Service.Implementations;

public class TransactionService : BaseService, ITransactionService
{
    private readonly VnPaySettings _vnPaySettings;
    private readonly IVnPayService _vnPayService;
    private readonly ILogger<TransactionService> _logger;

    private readonly IRepository<Transaction> _transactionRepo;
    private readonly IRepository<Booking> _bookingRepo;

    public TransactionService(IUnitOfWork unitOfWork, IOptions<VnPaySettings> vnPaySettings,
        IVnPayService vnPayService, ILogger<TransactionService> logger) : base(unitOfWork)
    {
        _vnPayService = vnPayService;
        _logger = logger;
        _transactionRepo = unitOfWork.Repo<Transaction>();
        _bookingRepo = unitOfWork.Repo<Booking>();
        _vnPaySettings = vnPaySettings.Value;
    }

    public async Task<Result<TransactionViewModel>> CreateTransaction(Guid bookingId, string clientIp)
    {
        var booking = await _bookingRepo.FindAsync(bookingId);
        if (booking is null) return Error.NotFound("Booking not found");
        if (booking.PaymentStatus is PaymentStatus.Paid) return Error.Conflict("Booking already paid");

        var amount = _calcAmount(booking);
        await using var tx = UnitOfWork.BeginTransaction();

        try
        {
            var newTransaction = _transactionRepo.Add(new Transaction()
            {
                BookingId = bookingId,
                Amount = amount,
                Status = TransactionStatus.Pending,
                Timestamp = DateTimeHelper.VnNow(),
            });
            await UnitOfWork.SaveChangesAsync();

            var requestModel = _buildVnPayRequestModel(newTransaction.Id, clientIp,
                amount, bookingId, booking.TravelerId, _vnPaySettings);

            var createPayRequestResult = await _vnPayService.CreateRequest(requestModel);
            if (!createPayRequestResult.IsSuccess)
                return Error.Unexpected("Create pay request failed");

            await tx.CommitAsync();

            // Result
            var view = newTransaction.Adapt<TransactionViewModel>();
            view.VnPayRequestId = requestModel.TxnRef;
            view.PayUrl = VnPay.CreateRequestUrl(requestModel, _vnPaySettings.BaseUrl, _vnPaySettings.HashSecret);
            return view;
        }
        catch (Exception e)
        {
            await tx.RollbackAsync();
            _logger.LogError(e, "{Message}", e.Message);
            return Error.Unexpected(e.Message);
        }
    }

    private static VnPayRequestModel _buildVnPayRequestModel(
        Guid transactionId, string clientIp, int amount, Guid bookingId, Guid travelerId, VnPaySettings settings)
    {
        var now = DateTimeHelper.VnNow();

        return new VnPayRequestModel()
        {
            TxnRef = Guid.NewGuid(),
            Command = VnPayConstants.Command,
            Locale = VnPayConstants.Locale,
            Version = VnPayConstants.Version,
            CurrencyCode = VnPayConstants.CurrencyCode,
            Amount = amount,
            CreateDate = now,
            ExpireDate = now.AddMinutes(15),
            OrderInfo = $"traveler '{travelerId}' pay tour booking {bookingId}, amount = {amount} vnd",
            IpAddress = clientIp,
            ReturnUrl = settings.ReturnUrl,
            TmnCode = settings.TmnCode,
            TransactionId = transactionId
        };
    }

    private static int _calcAmount(Booking booking)
    {
        return (booking.AdultPrice * booking.AdultQuantity) +
               (booking.ChildrenPrice * booking.ChildrenQuantity) +
               (booking.InfantPrice * booking.InfantQuantity);
    }
}
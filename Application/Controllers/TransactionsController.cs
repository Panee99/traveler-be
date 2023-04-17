using System.Globalization;
using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Models.Transaction;
using Service.Models.VnPay;
using Shared.ExternalServices.VnPay;
using Shared.ResultExtensions;
using Shared.Settings;

namespace Application.Controllers;

[Route("transactions")]
public class TransactionsController : ApiController
{
    private readonly VnPaySettings _vnPaySettings;
    private readonly ITransactionService _transactionService;
    private readonly IVnPayService _vnPayService;

    public TransactionsController(
        IOptions<VnPaySettings> vnPaySettings,
        ITransactionService transactionService,
        IVnPayService vnPayService)
    {
        _transactionService = transactionService;
        _vnPayService = vnPayService;
        _vnPaySettings = vnPaySettings.Value;
    }

    [Authorize(AccountRole.Traveler)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TransactionCreateModel model)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        if (clientIp is null) return OnError(Error.Unexpected("Client ip unknown"));

        var result = await _transactionService.CreateTransaction(model.BookingId, clientIp);
        return result.Match(Ok, OnError);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("ipn")]
    public async Task<IActionResult> VnPayIpnEntry([FromQuery] Dictionary<string, string> queryParams)
    {
        try
        {
            // TODO: return RspCode, Message 
            if (!VnPay.ValidateSignature(_vnPaySettings.HashSecret, queryParams))
                return BadRequest("Invalid Signature.");

            var model = VnPay.ParseToResponseModel(queryParams);
            var result = await _vnPayService.HandleResponse(model);
            return result.Match(Ok, OnError);
        }
        catch (Exception e)
        {
            return OnError(Error.Unexpected(e.Message));
        }
    }

    [HttpGet("result")]
    public IActionResult PaymentResult([FromQuery] Dictionary<string, string> queryParams)
    {
        if (!VnPay.ValidateSignature(_vnPaySettings.HashSecret, queryParams))
            return BadRequest("Invalid Signature.");
        var model = VnPay.ParseToResponseModel(queryParams);

        DateTime? payDate = model.PayDate is null
            ? null
            : DateTime.ParseExact(model.PayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        return Ok(new PaymentResultModel()
        {
            TransactionStatus = model.TransactionStatus,
            Response = model.ResponseCode,
            OrderInfo = model.OrderInfo,
            BankCode = model.BankCode,
            Amount = model.Amount,
            CardType = model.CardType,
            PayDate = payDate,
            TransactionNo = model.TransactionNo
        });
    }
}
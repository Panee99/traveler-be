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

    private readonly IVnPayService _vnPayService;

    public TransactionsController(
        IOptions<VnPaySettings> vnPaySettings,
        IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
        _vnPaySettings = vnPaySettings.Value;
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

    [ApiExplorerSettings(IgnoreApi = true)]
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
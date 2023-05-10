using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.VnPay;
using Shared.ExternalServices.VnPay;
using Shared.ResultExtensions;

namespace Application.Controllers;

[Route("transactions")]
public class TransactionsController : ApiController
{
    private readonly IVnPayService _vnPayService;
    private readonly VnPay _vnPay;

    public TransactionsController(IVnPayService vnPayService, VnPay vnPay)
    {
        _vnPayService = vnPayService;
        _vnPay = vnPay;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("ipn")]
    public async Task<IActionResult> VnPayIpnEntry([FromQuery] Dictionary<string, string> queryParams)
    {
        try
        {
            // TODO: return RspCode, Message 
            var parseResult = _vnPay.ParseToResponseModel(queryParams);
            if (!parseResult.IsSuccess) return BadRequest();

            var result = await _vnPayService.HandleResponse(parseResult.Value);
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
        var parseResult = _vnPay.ParseToResponseModel(queryParams);
        if (!parseResult.IsSuccess) return BadRequest();

        var model = parseResult.Value;
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
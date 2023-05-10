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
    private readonly VnPay _vnPay;
    private readonly ITransactionService _transactionService;
    
    public TransactionsController(VnPay vnPay, ITransactionService transactionService)
    {
        _vnPay = vnPay;
        _transactionService = transactionService;
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

            var result = await _transactionService.HandleIpnResponse(parseResult.Value);
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
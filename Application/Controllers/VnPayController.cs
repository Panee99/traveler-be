using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Models.VnPay;
using Shared.ExternalServices.VnPay;
using Shared.ExternalServices.VnPay.Models;
using Shared.Helpers;
using Shared.Settings;

namespace Application.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("pay")]
public class VnPayController : ApiController
{
    private readonly IVnPayRequestService _vnPayRequestService;
    private readonly IVnPayResponseService _vnPayResponseService;
    private readonly VnPaySettings _vnPaySettings;

    public VnPayController(IOptions<VnPaySettings> vnPaySettings,
        IVnPayRequestService vnPayRequestService, IVnPayResponseService vnPayResponseService)
    {
        _vnPayRequestService = vnPayRequestService;
        _vnPayResponseService = vnPayResponseService;
        _vnPaySettings = vnPaySettings.Value;
    }

    [HttpPost("request")]
    public async Task<IActionResult> CreatePayRequest(VnPayInputModel input)
    {
        var now = DateTimeHelper.VnNow();
        var clientIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
        var requestModel = new VnPayRequestModel
        {
            TxnRef = Guid.NewGuid(),
            Command = VnPayConstants.Command,
            Locale = VnPayConstants.Locale,
            Version = VnPayConstants.Version,
            CurrencyCode = VnPayConstants.CurrencyCode,
            Amount = input.Amount,
            CreateDate = now,
            ExpireDate = now.AddMinutes(15),
            OrderInfo = $"User A pay booking B: {input.Amount} VND",
            IpAddress = clientIp,
            ReturnUrl = _vnPaySettings.ReturnUrl,
            TmnCode = _vnPaySettings.TmnCode
        };

        var result = await _vnPayRequestService.Add(requestModel);
        return result.Match(() =>
        {
            var url = VnPay.CreateRequestUrl(requestModel, _vnPaySettings.BaseUrl, _vnPaySettings.HashSecret);
            return Ok(url);
        }, OnError);
    }

    [HttpGet("ipn")]
    public async Task<IActionResult> VnPayIpnEntry([FromQuery] Dictionary<string, string> queryParams)
    {
        // TODO: return RspCode, Message 
        if (!VnPay.ValidateSignature(_vnPaySettings.HashSecret, queryParams))
            return BadRequest("Invalid Signature.");

        var model = VnPay.ParseToResponseModel(queryParams);
        var result = await _vnPayResponseService.Add(model);
        return result.Match(Ok, OnError);
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

        return Ok(new PaymentResultModel
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
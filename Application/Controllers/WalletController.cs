using Data.Models.Create;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Settings;
using Shared.VnPay;
using Shared.VnPay.Models;

namespace Application.Controllers;

[Route("wallet")]
public class WalletController : ApiController
{
    private readonly VnPaySettings _vnPaySettings;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IOptions<VnPaySettings> vnPaySettings, ILogger<WalletController> logger) : base(logger)
    {
        _logger = logger;
        _vnPaySettings = vnPaySettings.Value;
    }

    [HttpPost("deposit")]
    public IActionResult Deposit(DepositModel model)
    {
        var now = DateTime.Now;
        var vnPayRequest = new VnPayRequestModel()
        {
            Amount = model.Amount,
            Command = "pay",
            CreateDate = now,
            ExpireDate = now.AddMinutes(15),
            OrderInfo = "Wallet deposit, User A",
            IpAddress = "171.249.233.160",
            ReturnUrl = "https://truongnx26.com",
            TmnCode = _vnPaySettings.TmnCode,
            TxnRef = Guid.NewGuid().ToString(),
        };

        var url = VnPayLibrary.CreateRequestUrl(vnPayRequest, _vnPaySettings.Url, _vnPaySettings.HashSecret);

        return Ok(url);
    }

    [HttpGet]
    public IActionResult DepositResponse([FromQuery] Dictionary<string, string> queryParams)
    {
        return Ok();
    }

    private void _readParams(Dictionary<string, string> queryParams)
    {
    }
}
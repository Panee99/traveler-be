using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.FcmToken;

namespace Application.Controllers;

[Route("fcm-tokens")]
public class FcmTokensController : ApiController
{
    private readonly IFcmTokenService _fcmTokenService;

    public FcmTokensController(IFcmTokenService fcmTokenService)
    {
        _fcmTokenService = fcmTokenService;
    }

    [ProducesResponseType(typeof(FcmTokenViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(FcmTokenCreateModel model)
    {
        var result = await _fcmTokenService.Create(model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _fcmTokenService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(List<FcmTokenViewModel>), StatusCodes.Status200OK)]
    [HttpPost("find-by-users")]
    public async Task<IActionResult> FindTokens(List<Guid> userIds)
    {
        var result = await _fcmTokenService.FindTokens(userIds);
        return result.Match(Ok, OnError);
    }
}
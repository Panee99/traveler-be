using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourFlow;

namespace Application.Controllers;

[Authorize(AccountRole.Manager)]
[Route("tour-flows")]
public class TourFlowsController : ApiController
{
    private readonly ITourFlowService _tourFlowService;

    public TourFlowsController(ITourFlowService tourFlowService)
    {
        _tourFlowService = tourFlowService;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(TourFlowCreateModel model)
    {
        var result = await _tourFlowService.Create(model);
        return result.Match(Ok, OnError);
    }

    [HttpPatch("")]
    public async Task<IActionResult> Update(
        [FromQuery] Guid tourId,
        [FromQuery] Guid locationId,
        [FromBody] TourFlowUpdateModel model)
    {
        var result = await _tourFlowService.Update(tourId, locationId, model);
        return result.Match(Ok, OnError);
    }

    [HttpDelete("")]
    public async Task<IActionResult> Delete([FromQuery] Guid tourId, [FromQuery] Guid locationId)
    {
        var result = await _tourFlowService.Delete(tourId, locationId);
        return result.Match(Ok, OnError);
    }

    [AllowAnonymous]
    [HttpGet("tours/{tourId:guid}/tour-flows")]
    public async Task<IActionResult> ListByTour(Guid tourId)
    {
        var result = await _tourFlowService.ListByTour(tourId);
        return result.Match(Ok, OnError);
    }
}
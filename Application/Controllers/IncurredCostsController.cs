using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.InccuredCost;

namespace Application.Controllers;

// TODO: refactor
[Authorize(UserRole.Admin, UserRole.Traveler)]
[Route("incurred-costs")]
public class IncurredCostsController : ApiController
{
    private readonly IIncurredCostService _incurredCostService;

    public IncurredCostsController(IIncurredCostService incurredCostService)
    {
        _incurredCostService = incurredCostService;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(IncurredCostCreateModel model)
    {
        var result = await _incurredCostService.Create(model);
        return result.Match(Ok, OnError);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _incurredCostService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [HttpGet("")]
    public async Task<IActionResult> List([FromQuery] Guid tourId, [FromQuery] Guid tourGuideId)
    {
        var result = await _incurredCostService.List(tourId, tourGuideId);
        return result.Match(Ok, OnError);
    }
}
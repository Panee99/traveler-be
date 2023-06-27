using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.IncurredCost;

namespace Application.Controllers;

// TODO: refactor
[Authorize(UserRole.TourGuide)]
[Route("incurred-costs")]
public class IncurredCostsController : ApiController
{
    private readonly IIncurredCostService _incurredCostService;

    public IncurredCostsController(IIncurredCostService incurredCostService)
    {
        _incurredCostService = incurredCostService;
    }

    /// <summary>
    /// Create a new Incurred Cost
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Create(IncurredCostCreateModel model)
    {
        var result = await _incurredCostService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete an Incurred Cost
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _incurredCostService.Delete(id);
        return result.Match(Ok, OnError);
    }
}
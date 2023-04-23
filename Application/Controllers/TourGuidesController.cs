using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace Application.Controllers;

[Route("tour-guides")]
public class TourGuidesController : ApiController
{
    private readonly ITourGuideService _tourGuideService;

    public TourGuidesController(ITourGuideService tourGuideService)
    {
        _tourGuideService = tourGuideService;
    }

    [HttpGet("{id:guid}/assigned-tours")]
    public async Task<IActionResult> ListAssignedTours(Guid id)
    {
        var result = await _tourGuideService.ListAssignedTours(id);
        return result.Match(Ok, OnError);
    }
}
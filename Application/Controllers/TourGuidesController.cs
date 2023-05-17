using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGuide;

namespace Application.Controllers;

[Route("tour-guides")]
public class TourGuidesController : ApiController
{
    private readonly ITourGuideService _tourGuideService;

    public TourGuidesController(ITourGuideService tourGuideService)
    {
        _tourGuideService = tourGuideService;
    }

    [Authorize(UserRole.Admin)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourGuideCreateModel model)
    {
        var result = await _tourGuideService.Create(model);
        return result.Match(Ok, OnError);
    }

    [HttpGet("{id:guid}/assigned-tours")]
    public async Task<IActionResult> ListAssignedTours(Guid id)
    {
        var result = await _tourGuideService.ListAssignedTours(id);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Admin)]
    [HttpGet("")]
    public async Task<IActionResult> ListAll()
    {
        var result = await _tourGuideService.ListAll();
        return result.Match(Ok, OnError);
    }
}
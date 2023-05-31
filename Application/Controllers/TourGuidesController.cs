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

    /// <summary>
    /// List all tours this tour guide assigned to
    /// </summary>
    [HttpGet("{id:guid}/assigned-tours")]
    public async Task<IActionResult> ListAssignedTours(Guid id)
    {
        var result = await _tourGuideService.ListAssignedTours(id);
        return result.Match(Ok, OnError);
    }
    
    /// <summary>
    /// List all tour groups this tour guide assigned to
    /// </summary>
    [HttpGet("{id:guid}/assigned-groups")]
    public async Task<IActionResult> ListAssignedTourGroups(Guid id)
    {
        var result = await _tourGuideService.ListAssignedGroups(id);
        return result.Match(Ok, OnError);
    }
}
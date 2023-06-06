using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;

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
    /// List all tour groups this tour guide assigned to
    /// </summary>
    [Authorize(UserRole.Admin, UserRole.TourGuide)]
    [ProducesResponseType(typeof(List<TourGroupViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/assigned-groups")]
    public async Task<IActionResult> ListAssignedTourGroups(Guid id)
    {
        var result = await _tourGuideService.ListAssignedGroups(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all tour groups this tour guide assigned to
    /// </summary>
    [Authorize(UserRole.Admin, UserRole.TourGuide)]
    [ProducesResponseType(typeof(TourGroupViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/current-group")]
    public async Task<IActionResult> GetCurrentAssignedTourGroup(Guid id)
    {
        var result = await _tourGuideService.GetCurrentAssignedTourGroup(id);
        return result.Match(Ok, OnError);
    }
}
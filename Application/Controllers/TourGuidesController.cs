using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
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
    /// List all tour groups this tour guide assigned to
    /// </summary>
    [Authorize(UserRole.Manager, UserRole.TourGuide)]
    [ProducesResponseType(typeof(List<TourGroupViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/assigned-groups")]
    public async Task<IActionResult> ListAssignedTourGroups(Guid id)
    {
        var result = await _tourGuideService.ListAssignedGroups(id);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager, UserRole.TourGuide)]
    [ProducesResponseType(typeof(TourGuideViewModel), StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/contacts")]
    public async Task<IActionResult> UpdateContacts(Guid id, ContactsUpdateModel model)
    {
        var result = await _tourGuideService.UpdateContacts(id, model);
        return result.Match(Ok, OnError);
    }
}
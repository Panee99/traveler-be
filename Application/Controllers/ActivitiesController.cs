using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Activity;

namespace Application.Controllers;

[Route("activities")]
public class ActivitiesController : ApiController
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(ActivityCreateModel model)
    {
        var result = await _activityService.Create(model);
        return result.Match(Ok, OnError);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _activityService.Delete(id);
        return result.Match(Ok, OnError);
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActivityUpdateModel model)
    {
        var result = await _activityService.Update(id, model);
        return result.Match(Ok, OnError);
    }
}
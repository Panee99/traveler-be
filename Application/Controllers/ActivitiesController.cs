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

    // [HttpPost("{id:guid}/attendances")]
    // public async Task<IActionResult> CreateAttendance(Guid id, AttendanceCreateModel model)
    // {
    //     var result = await _activityService.CreateAttendance(id, model);
    //     return result.Match(Ok, OnError);
    // }
    //
    // [HttpGet("{id:guid}/attendances")]
    // public async Task<IActionResult> ListAttendances(Guid id)
    // {
    //     var result = await _activityService.ListAttendances(id);
    //     return result.Match(Ok, OnError);
    // }
}
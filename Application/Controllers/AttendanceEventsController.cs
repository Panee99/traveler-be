using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attendance;
using Service.Models.AttendanceEvent;

namespace Application.Controllers;

[Route("attendance-events")]
public class AttendanceEventsController : ApiController
{
    private readonly IAttendanceEventService _attendanceEventService;

    public AttendanceEventsController(IAttendanceEventService attendanceEventService)
    {
        _attendanceEventService = attendanceEventService;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(AttendanceEventCreateModel model)
    {
        var result = await _attendanceEventService.Create(model);
        return result.Match(Ok, OnError);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _attendanceEventService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [HttpPost("{id:guid}/attendances")]
    public async Task<IActionResult> CreateAttendance(Guid id, AttendanceCreateModel model)
    {
        var result = await _attendanceEventService.CreateAttendance(id, model);
        return result.Match(Ok, OnError);
    }

    [HttpGet("{id:guid}/attendances")]
    public async Task<IActionResult> ListAttendances(Guid id)
    {
        var result = await _attendanceEventService.ListAttendances(id);
        return result.Match(Ok, OnError);
    }
}
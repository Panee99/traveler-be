using System.Net;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Location;
using Shared.Enums;

namespace Application.Controllers;

[Route("locations")]
public class LocationsController : ApiController
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public async Task<IActionResult> Create(LocationCreateModel model)
    {
        var result = await _locationService.Create(model);
        return result.Match(
            value => new ObjectResult(value) { StatusCode = (int)HttpStatusCode.Created },
            OnError);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _locationService.Find(id);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.Delete(id);
        return result.Match(Ok, OnError);
    }
}
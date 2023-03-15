using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Location;

namespace Application.Controllers;

[Route("locations")]
public class LocationController : ApiController
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpPost("")]
    public IActionResult Create(LocationCreateModel model)
    {
        return _locationService.Create(model)
            .Match(
                value => CreatedAtAction(nameof(Find), new { Id = value }, null),
                OnError);
    }

    [HttpGet("{id:guid}")]
    public IActionResult Find([FromRoute] Guid id)
    {
        var result = _locationService
            .Find(id);
        
        return result.Match(Ok, OnError);
    }
}
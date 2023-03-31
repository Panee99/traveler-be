﻿using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
using Shared.Enums;

namespace Application.Controllers;

[Route("tour-groups")]
public class TourGroupsController : ApiController
{
    private readonly ITourGroupService _tourGroupService;

    public TourGroupsController(ITourGroupService tourGroupService)
    {
        _tourGroupService = tourGroupService;
    }

    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourGroupCreateModel model)
    {
        var result = await _tourGroupService.Create(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, TourGroupUpdateModel model)
    {
        var result = await _tourGroupService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _tourGroupService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [HttpPatch("{id:guid}/travelers")]
    public async Task<IActionResult> AddTravelers([FromRoute] Guid id, [FromBody] List<Guid> travelerIds)
    {
        var result = await _tourGroupService.AddTravelers(id, travelerIds);
        return result.Match(Ok, OnError);
    }

    [HttpDelete("{id:guid}/travelers")]
    public async Task<IActionResult> RemoveTravelers([FromRoute] Guid id, [FromBody] List<Guid> travelerIds)
    {
        var result = await _tourGroupService.RemoveTravelers(id, travelerIds);
        return result.Match(Ok, OnError);
    }

    [HttpGet("{id:guid}/travelers")]
    public async Task<IActionResult> ListTravelers([FromRoute] Guid id)
    {
        var result = await _tourGroupService.ListTravelers(id);
        return result.Match(Ok, OnError);
    }
}
﻿using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Traveler;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("travelers")]
public class TravelersController : ApiController
{
    private readonly ITravelerService _travelerService;

    public TravelersController(ITravelerService travelerService)
    {
        _travelerService = travelerService;
    }

    /// <summary>
    /// List all joined groups
    /// </summary>
    [Authorize(UserRole.Traveler)]
    [ProducesResponseType(typeof(TourGroupViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/joined-groups")]
    public async Task<IActionResult> ListJoinedGroups(Guid id)
    {
        var result = await _travelerService.ListJoinedGroups(id);
        return result.Match(Ok, OnError);
    }
}
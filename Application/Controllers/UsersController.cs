﻿using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.User;

namespace Application.Controllers;


[Route("users")]
public class UsersController : ApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(UserRole.Admin)]
    [HttpPost("")]
    public async Task<IActionResult> Create(UserCreateModel model)
    {
        var result = await _userService.Create(model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Admin)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateModel model)
    {
        var result = await _userService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Admin)]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(UserFilterModel model)
    {
        var result = await _userService.Filter(model);
        return result.Match(Ok, OnError);
    }
    
    [Authorize]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpGet("self/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfile(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }
    
    [Authorize]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpPatch("self/profile")]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateModel model)
    {
        var result = await _userService.UpdateProfile(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }
}
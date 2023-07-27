﻿using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
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

    /// <summary>
    /// Change user password
    /// </summary>
    [Authorize]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpPut("self/password")]
    public async Task<IActionResult> ChangePassword(PasswordUpdateModel model)
    {
        var result = await _userService.ChangePassword(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [Authorize]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpGet("self/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfile(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [Authorize]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpPatch("self/profile")]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateModel model)
    {
        var result = await _userService.UpdateProfile(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    [Authorize(UserRole.Manager)]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> MangerGetUserById(Guid id)
    {
        var result = await _userService.AdminGetUserById(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get travel info
    /// </summary>
    [ProducesResponseType(typeof(TravelInfo), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/travel-info")]
    public async Task<IActionResult> GetTravelInfo(Guid id)
    {
        var result = await _userService.GetTravelInfo(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get current joined group
    /// </summary>
    [ProducesResponseType(typeof(CurrentTourGroupViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/current-group")]
    public async Task<IActionResult> GetCurrentJoinedGroup(Guid id)
    {
        var result = await _userService.GetCurrentJoinedGroup(id);
        return result.Match(Ok, OnError);
    }

    // #region Required Manager Role
    //
    // /// <summary>
    // /// Create new user
    // /// </summary>
    // [Authorize(UserRole.Manager)]
    // [HttpPost("")]
    // public async Task<IActionResult> Create(UserCreateModel model)
    // {
    //     var result = await _userService.Create(model);
    //     return result.Match(Ok, OnError);
    // }
    //
    // /// <summary>
    // /// Update user
    // /// </summary>
    // [Authorize(UserRole.Manager)]
    // [HttpPatch("{id:guid}")]
    // public async Task<IActionResult> Update(Guid id, UserUpdateModel model)
    // {
    //     var result = await _userService.Update(id, model);
    //     return result.Match(Ok, OnError);
    // }
    //
    // /// <summary>
    // /// Filter users
    // /// </summary>
    // [Authorize(UserRole.Manager)]
    // [HttpPost("filter")]
    // public async Task<IActionResult> Filter(UserFilterModel model)
    // {
    //     var result = await _userService.Filter(model);
    //     return result.Match(Ok, OnError);
    // }
    //
    // /// <summary>
    // /// Delete a user
    // /// </summary>
    // [Authorize(UserRole.Manager)]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [HttpDelete("{id:guid}")]
    // public async Task<IActionResult> AdminDeleteUserById(Guid id)
    // {
    //     var result = await _userService.AdminDeleteUserById(id);
    //     return result.Match(Ok, OnError);
    // }
    //
    // #endregion

}
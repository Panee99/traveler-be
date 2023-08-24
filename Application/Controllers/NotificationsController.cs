using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Notification;

namespace Application.Controllers;

[Authorize]
[Route("notifications")]
public class NotificationsController : ApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get all user's notifications
    /// </summary>
    [ProducesResponseType(typeof(List<NotificationViewModel>), StatusCodes.Status200OK)]
    [HttpGet("")]
    public async Task<IActionResult> ListAll([FromQuery] Guid tripId)
    {
        var result = await _notificationService.ListAll(CurrentUser.Id, tripId);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Read a notification
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id)
    {
        var result = await _notificationService.MarkAsRead(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Read all user's notifications
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        var result = await _notificationService.MarkAllAsRead(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get user's notifications unread count
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notificationService.GetUnreadCount(CurrentUser.Id);
        return result.IsSuccess ? Ok(new { Count = result.Value }) : OnError(result.Error);
    }
}
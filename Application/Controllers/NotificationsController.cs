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

    [ProducesResponseType(typeof(List<NotificationViewModel>), StatusCodes.Status200OK)]
    [HttpGet("")]
    public async Task<IActionResult> ListAll()
    {
        var result = await _notificationService.ListAll(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }
}
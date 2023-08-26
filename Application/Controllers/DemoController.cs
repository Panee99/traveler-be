using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Channels.Notification;
using Service.Interfaces;
using Shared.ResultExtensions;

namespace Application.Controllers;

[Route("demo")]
public class DemoController : ApiController
{
    private readonly UnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public DemoController(UnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    [HttpGet("weather-alert")]
    public async Task<IActionResult> DemoWeatherAlert(Guid tripId)
    {
        if (!await _unitOfWork.Trips.AnyAsync(e => e.Id == tripId))
            return OnError(Error.NotFound(DomainErrors.Trip.NotFound));

        var effective = DateTime.Today.AddDays(-1);
        var expires = DateTime.Today.AddDays(1);

        var alert = new WeatherAlert()
        {
            TripId = tripId,
            Headline =
                $"Flood Warning issued {effective.ToString("MMMM d 'at' h:mmtt")} until {expires.ToString("MMMM d 'at' h:mmtt")} by NWS",
            Areas = "Sing Buri, Ang Thong",
            Certainty = "Likely",
            Description =
                "The National Weather Service in Thailand has issued a " +
                "Flood Warning for the following rivers in Sing Buri, Ang Thong.",
            Effective = effective,
            Expires = expires,
            Urgency = "Expected",
            Event = "Flood Warning",
            Instruction =
                "Caution is urged when walking near riverbanks. " +
                "Be especially cautious at night when it is harder to recognize the " +
                "dangers of flooding. Turn around",
            Severity = "Moderate",
            Note = "Alert for Sing Buri, Ang Thong Issued by the National Weather Service"
        };

        // Remove old records
        _unitOfWork.WeatherAlerts.RemoveRange(
            _unitOfWork.WeatherAlerts.Query().Where(e => e.TripId == tripId));

        // Add new
        _unitOfWork.WeatherAlerts.Add(alert);
        await _unitOfWork.SaveChangesAsync();

        // Send notification
        var groups = await _unitOfWork.TourGroups.Query()
            .Where(g => g.TripId == tripId)
            .Select(g => new
            {
                g.TourGuideId,
                TravelerIds = g.Travelers.Select(t => t.Id)
            })
            .ToListAsync();

        var receiverIds = groups.Select(x =>
            {
                var ids = x.TravelerIds.ToList();
                if (x.TourGuideId != null) ids.Add(x.TourGuideId.Value);
                return ids;
            })
            .SelectMany(x => x)
            .ToList();

        await _notificationService.EnqueueNotification(new NotificationJob(
            tripId, receiverIds, NotificationType.WeatherAlert, null,
            alert.Event, alert.Headline));

        return Ok();
    }
}
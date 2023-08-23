using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Interfaces;

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
        var alert = new WeatherAlert()
        {
            TripId = tripId,
            Headline = "Flood Warning issued July 16 at 7:51PM EDT until July 18 at 8:00AM EDT by NWS",
            Areas = "Hampden",
            Certainty = "Likely",
            Description =
                @"...The National Weather Service in Boston/Norton MA has issued a\\nFlood Warning for the following rivers in Massachusetts...\\nConnecticut...Rhode Island...\\nConnecticut River At Thompsonville affecting Hampden and Hartford\\nCounties.\\nFarmington River At Simsbury affecting Hartford County.\\nWood River At Hope Valley affecting Washington County.\\nFor the Connecticut River...including Montague",
            Effective = DateTime.Parse("2023-07-17 15:00:00.0000000"),
            Expires = DateTime.Parse("2023-07-18 12:00:00.0000000"),
            Urgency = "Expected",
            Event = "Flood Warning",
            Instruction =
                @"Caution is urged when walking near riverbanks.\\nBe especially cautious at night when it is harder to recognize the\\ndangers of flooding.\\nTurn around",
            Severity = "Moderate",
            Note = "Alert for Hampden (Massachusetts) Issued by the National Weather Service"
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
            receiverIds, NotificationType.WeatherAlert, null, 
            alert.Event, alert.Headline));

        return Ok();
    }
}
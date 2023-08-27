using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Interfaces;

namespace Application.Pages.Manager;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly UnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public IndexModel(UnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Logout
    /// </summary>
    public IActionResult OnGetLogout()
    {
        HttpContext.Session.Remove("User");
        return RedirectToPage("Login");
    }

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public WeatherAlert? WeatherAlert { get; set; }

    public async Task OnGetAsync()
    {
        await _loadPageData();
    }

    public async Task<IActionResult> OnPostWeatherAlertTrigger()
    {
        await _loadPageData();

        var x = Request.Form["trip"].FirstOrDefault();
        var tripId = Guid.Parse(x ?? "");

        if (!await _unitOfWork.Trips.AnyAsync(e => e.Id == tripId)) return Page();

        var effective = DateTime.Today.AddDays(-1);
        var expires = DateTime.Today.AddDays(1);

        var alert = new WeatherAlert()
        {
            TripId = tripId,
            Headline =
                $"Flood Warning issued {effective.ToString("MMMM d 'at' h:mmtt")} until " +
                $"{expires.ToString("MMMM d 'at' h:mmtt")} by NWS",
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

        WeatherAlert = alert;

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

        return Page();
    }

    private async Task _loadPageData()
    {
        Trips = await _unitOfWork.Trips.Query()
            .Include(e => e.Tour)
            .Where(e => e.DeletedById == null &&
                        e.Tour.DeletedById == null)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();
    }
}
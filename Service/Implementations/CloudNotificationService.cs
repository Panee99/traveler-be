using System.Globalization;
using Data.Enums;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Service.Commons;
using Service.Interfaces;
using Shared.ExternalServices.Firebase;
using Shared.Helpers;

namespace Service.Implementations;

public class CloudNotificationService : ICloudNotificationService
{
    private readonly ILogger<CloudNotificationService> _logger;
    private readonly FirebaseMessaging _messaging;
    private readonly ICloudStorageService _cloudStorageService;

    public CloudNotificationService(ILogger<CloudNotificationService> logger, ICloudStorageService cloudStorageService)
    {
        _logger = logger;
        _cloudStorageService = cloudStorageService;
        _messaging = FirebaseAppHelper.GetMessaging();
    }

    public async Task SendBatchMessages(
        ICollection<string> tokens,
        string title,
        string payload,
        NotificationType type)
    {
        if (tokens.Count == 0) return;

        var messages = tokens.Select(token => _buildFirebaseMessage(token, title, payload, type));

        try
        {
            var batchResponse = await _messaging.SendAllAsync(messages);

            if (batchResponse.FailureCount == 0) return;

            foreach (var response in batchResponse.Responses)
            {
                if (response.IsSuccess) continue;

                _logger.LogError(response.Exception,
                    "Sending attendance notification failed: {Message}",
                    response.Exception.Message);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending batch notifications: {Message}", e.Message);
        }
    }

    private Message _buildFirebaseMessage(
        string token,
        string title,
        string payload,
        NotificationType type)
    {
        var iconId = type switch
        {
            NotificationType.AttendanceEvent => ServiceConstants.AttendanceImage,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Message()
        {
            Token = token,
            Android = new AndroidConfig()
            {
                Data = new Dictionary<string, string>
                {
                    { "id", Guid.NewGuid().ToString() },
                    {
                        "timestamp",
                        DateTimeHelper.VnNow().ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture)
                    },
                    { "type", type.ToString() }
                },
                Notification = new AndroidNotification()
                {
                    Title = title,
                    Body = payload,
                    EventTimestamp = DateTimeHelper.VnNow(),
                    Priority = NotificationPriority.HIGH,
                    ClickAction = type switch
                    {
                        NotificationType.AttendanceEvent => NotificationAction.Attendance,
                        _ => null
                    },
                    ImageUrl = _cloudStorageService.GetMediaLink(iconId),
                },
            }
        };
    }
}

internal struct NotificationAction
{
    public const string Warning = "ACTION_WARNING";
    public const string Attendance = "ACTION_ATTENDENCE";
    public const string StartTour = "ACTION_START_TOUR";
    public const string AcceptTour = "ACTION_TRAVELLER_ACCEPT_TOUR";
}
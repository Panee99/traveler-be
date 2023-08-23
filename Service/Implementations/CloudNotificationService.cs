using System.Globalization;
using Data.Enums;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
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
        NotificationType type,
        string title,
        string payload)
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
                if (response.Exception.ErrorCode is ErrorCode.NotFound) continue;

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
                        NotificationType.AttendanceActivity => NotificationAction.Attendance,
                        NotificationType.WeatherAlert => NotificationAction.Warning,
                        _ => null
                    }
                },
            }
        };
    }
}

internal struct NotificationAction
{
    public const string Warning = "ACTION_WARNING";
    public const string Attendance = "ACTION_ATTENDENCE";
}
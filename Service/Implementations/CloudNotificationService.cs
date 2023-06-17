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

    public CloudNotificationService(ILogger<CloudNotificationService> logger)
    {
        _logger = logger;
        _messaging = FirebaseAppHelper.GetMessaging();
    }

    public async Task SendBatchMessages(ICollection<string> tokens, string title, string payload, string type)
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

    private static Message _buildFirebaseMessage(string token, string title, string payload, string type)
    {
        return new Message()
        {
            Token = token,
            Android = new AndroidConfig()
            {
                Data = new Dictionary<string, string>
                {
                    { "id", Guid.NewGuid().ToString() },
                    { "timestamp", DateTimeHelper.GetUtcTimestamp().ToString() },
                    { "type", type }
                },
                Notification = new AndroidNotification()
                {
                    Title = title,
                    Body = payload,
                },
            }
        };
    }
}
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

    public void SendBatchMessages(ICollection<string> tokens, string title, string payload)
    {
        if (tokens.Count == 0) return;

        _ = Task.Run(async () =>
        {
            var messages = tokens.Select(token => _buildFirebaseMessage(token, title, payload));

            try
            {
                var batchResponse = await _messaging.SendAllAsync(messages);

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
        });
    }

    private static Message _buildFirebaseMessage(string token, string title, string payload)
    {
        return new Message()
        {
            Token = token,
            Android = new AndroidConfig()
            {
                Data = new Dictionary<string, string>
                {
                    { "id", Guid.NewGuid().ToString() },
                    { "timestamp", DateTimeHelper.GetUtcTimestamp().ToString() }
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
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Shared.Firebase;

namespace Service.Implementations;

public class CloudMessagingService : ICloudMessagingService
{
    private static readonly FirebaseMessaging Messaging;
    private readonly ILogger<CloudMessagingService> _logger;
    
    static CloudMessagingService()
    {
        Console.WriteLine(FirebaseApp.DefaultInstance);
        if (FirebaseApp.DefaultInstance is null) FirebaseAppInitializer.Init();
        Messaging = FirebaseMessaging.DefaultInstance;
    }
    
    public CloudMessagingService(ILogger<CloudMessagingService> logger)
    {
        _logger = logger;
    }
}
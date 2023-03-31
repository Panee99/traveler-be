using Data.EFCore;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Service.Interfaces;

namespace Service.Implementations;

public class CloudMessagingService : BaseService, ICloudMessagingService
{
    private static readonly FirebaseMessaging Messaging;
    private readonly ILogger<CloudMessagingService> _logger;

    static CloudMessagingService()
    {
        Messaging = FirebaseMessaging.DefaultInstance;
    }

    public CloudMessagingService(ILogger<CloudMessagingService> logger, IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _logger = logger;
    }
}
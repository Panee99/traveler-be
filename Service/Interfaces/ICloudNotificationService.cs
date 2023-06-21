﻿using Data.Enums;

namespace Service.Interfaces;

public interface ICloudNotificationService
{
    Task SendBatchMessages(ICollection<string> tokens, string title, string payload, NotificationType type);
}
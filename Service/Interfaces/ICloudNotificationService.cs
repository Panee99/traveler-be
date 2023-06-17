namespace Service.Interfaces;

public interface ICloudNotificationService
{
    void SendBatchMessages(ICollection<string> tokens, string title, string payload);
}
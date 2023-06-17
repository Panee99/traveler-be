using System.Threading.Channels;

namespace Shared.Channels.Notification;

public sealed class NotificationJobQueue : INotificationJobQueue
{
    private const int DefaultQueueCapacity = 10_000;

    private readonly int _capacity;
    private readonly Channel<NotificationJob> _queue;

    public NotificationJobQueue() : this(DefaultQueueCapacity)
    {
    }

    public NotificationJobQueue(int capacity)
    {
        _capacity = capacity;
        var options = new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<NotificationJob>(options);
    }

    public async ValueTask EnqueueAsync(NotificationJob job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<NotificationJob> DequeueAsync()
    {
        return await _queue.Reader.ReadAsync();
    }

    public int GetCapacity()
    {
        return _capacity;
    }
}
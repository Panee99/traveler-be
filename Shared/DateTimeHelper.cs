namespace Shared;

public static class DateTimeHelper
{
    public static DateTime VnNow()
    {
        return DateTime.UtcNow.AddHours(7);
    }
}
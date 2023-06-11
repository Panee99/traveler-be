namespace Service.Commons;

/// <summary>
/// Generate Tour Code
/// </summary>
/// <returns> Ex: 2305-37CCBD12 </returns>
public static class CodeGenerator
{
    private static readonly DateTime UnixStart = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private static readonly object Lock = new();
    private static long _lastTimestamp;
    
    public static string NewCode()
    {
        lock (Lock)
        {
            while (true)
            {
                var now = DateTime.UtcNow.AddHours(7);
                var currentTimestamp = (long)(now - UnixStart).TotalMilliseconds;

                if (_lastTimestamp >= currentTimestamp)
                {
                    _lastTimestamp = currentTimestamp;
                    continue;
                }

                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var monthElapsedTimestamp = currentTimestamp - (long)(startOfMonth - UnixStart).TotalMilliseconds;

                var firstPart = _getYearMonth(now);
                var secondPart = monthElapsedTimestamp.ToString("X");

                _lastTimestamp = currentTimestamp;
                return $"{firstPart}-{secondPart}";
            }
        }
    }

    private static string _getYearMonth(DateTime now)
    {
        var year = now.Year % 100;

        string month;
        if (now.Month < 10) month = "0" + now.Month;
        else month = now.Month.ToString();

        return $"{year}{month}";
    }
}
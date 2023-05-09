namespace Shared;

public class Snowflake
{
    private const uint SequenceBit = 12;

    private readonly ulong _sequenceMax = (ulong)Math.Pow(2, SequenceBit);

    private ulong _lastTimestamp;
    private ulong _sequence;

    public Snowflake()
    {
        _lastTimestamp = 0;
        _sequence = 0;
    }

    public long NewId()
    {
        ulong result = 0;

        var currentTimestamp = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
            .TotalMilliseconds;

        var cmp = currentTimestamp.CompareTo(_lastTimestamp);

        switch (cmp)
        {
            case 1:
            {
                _sequence = 0;
                result = Combine(currentTimestamp, _sequence);
                break;
            }
            case 0:
            {
                _sequence++;
                if (_sequence >= _sequenceMax)
                {
                    currentTimestamp = NextMillisecond();
                    _sequence = 0;
                }

                result = Combine(currentTimestamp, _sequence);
                break;
            }
            case -1:
            {
                currentTimestamp = _lastTimestamp;
                _sequence++;
                if (_sequence > _sequenceMax)
                {
                    currentTimestamp = NextMillisecond();
                    _sequence = 0;
                }

                result = Combine(currentTimestamp, _sequence);
                break;
            }
        }

        _lastTimestamp = currentTimestamp;
        return (long)result;
    }

    private static readonly DateTime UnixStart = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    private ulong NextMillisecond()
    {
        ulong timestamp;
        do
        {
            timestamp = (ulong)(DateTime.UtcNow - UnixStart).TotalMilliseconds;
        } while (timestamp <= _lastTimestamp);

        return timestamp;
    }

    private static ulong Combine(ulong timestamp, ulong sequence)
    {
        return (timestamp << (int)SequenceBit) | sequence;
    }
}
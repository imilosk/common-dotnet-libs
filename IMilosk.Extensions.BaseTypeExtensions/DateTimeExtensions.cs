namespace IMilosk.Extensions.BaseTypeExtensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimestampMilliseconds(this DateTime date)
    {
        return (long)AsTimeSpanFromUnixEpoch(date)
            .TotalMilliseconds;
    }

    public static TimeSpan AsTimeSpanFromUnixEpoch(this DateTime date)
    {
        return date.Subtract(DateTime.UnixEpoch);
    }
}
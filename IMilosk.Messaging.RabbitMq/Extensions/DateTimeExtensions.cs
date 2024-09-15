using RabbitMQ.Client;

namespace IMilosk.Messaging.RabbitMq.Extensions;

public static class DateTimeExtensions
{
    private static long ToUnixTimestampMilliseconds(this DateTime date)
    {
        return (long)AsTimeSpanFromUnixEpoch(date)
            .TotalMilliseconds;
    }

    private static TimeSpan AsTimeSpanFromUnixEpoch(this DateTime date)
    {
        return date.Subtract(DateTime.UnixEpoch);
    }

    public static AmqpTimestamp ToAmqpTimestamp(this DateTime dateTime)
    {
        return new AmqpTimestamp(dateTime.ToUnixTimestampMilliseconds());
    }
}
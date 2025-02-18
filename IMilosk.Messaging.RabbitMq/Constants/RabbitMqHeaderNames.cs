namespace IMilosk.Messaging.RabbitMq.Constants;

public static class RabbitMqHeaderNames
{
    public const string ContentType = "content-type";
    public const string UserAgent = "user-agent";
    public const string XMessageTtl = "x-message-ttl";
    public const string XDeadLetterExchange = "x-dead-letter-exchange";
    public const string XDeadLetterRoutingKey = "x-dead-letter-routing-key";
}
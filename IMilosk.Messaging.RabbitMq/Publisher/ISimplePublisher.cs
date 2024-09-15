namespace IMilosk.Messaging.RabbitMq.Publisher;

public interface ISimplePublisher : IDisposable
{
    bool PublishJsonMessage<T>(
        T messageObj,
        string exchange,
        string routingKey,
        string userAgent,
        int confirmationTimeout = 10
    );
}
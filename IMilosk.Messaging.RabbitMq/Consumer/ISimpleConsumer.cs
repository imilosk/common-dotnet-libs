using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IMilosk.Messaging.RabbitMq.Consumer;

public interface ISimpleConsumer : IDisposable
{
    IEnumerable<AsyncEventingBasicConsumer> CreateConsumers(
        int consumerCount,
        string queueName,
        string queueType,
        string routingKey,
        string exchange,
        Func<IModel, BasicDeliverEventArgs, Task> onMessageReceivedDelegate,
        uint prefetchSize = 0,
        ushort prefetchCount = 1,
        bool global = false
    );
}
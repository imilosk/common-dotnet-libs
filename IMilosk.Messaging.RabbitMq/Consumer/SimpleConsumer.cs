using IMilosk.Messaging.RabbitMq.Connector;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IMilosk.Messaging.RabbitMq.Consumer;

public class SimpleConsumer : ISimpleConsumer
{
    private readonly IConnection _rmqConnection;

    public SimpleConsumer(IRabbitMqConnector rabbitMqConnector)
    {
        _rmqConnection = rabbitMqConnector.GetConnection();
    }

    public void Dispose()
    {
        _rmqConnection.Close();

        GC.SuppressFinalize(this);
    }

    public IEnumerable<AsyncEventingBasicConsumer> CreateConsumers(
        int consumerCount,
        string queueName,
        string queueType,
        string routingKey,
        string exchange,
        Func<IModel, BasicDeliverEventArgs, Task> onMessageReceivedDelegate,
        uint prefetchSize = 0,
        ushort prefetchCount = 1,
        bool global = false
    )
    {
        var arguments = new Dictionary<string, object>
        {
            {
                Headers.XQueueType, queueType
            },
        };

        var consumers = new List<AsyncEventingBasicConsumer>(consumerCount);
        for (var count = 0; count < consumerCount; count++)
        {
            var channel = _rmqConnection.CreateModel();

            // Configure Quality of Service (QoS) to ensure efficient message distribution among multiple consumers:
            // - prefetchSize: allows consumers to prefetch messages without limiting message payload size; 
            // - prefetchCount: limits each consumer to receive only one message at a time from the queue;
            // - global is set to false, indicating these QoS settings apply only to the current channel;
            channel.BasicQos(prefetchSize, prefetchCount, global);

            var consumer = CreateAndRegisterConsumer(
                channel,
                queueName,
                routingKey,
                exchange,
                arguments,
                onMessageReceivedDelegate
            );
            consumers.Add(consumer);
        }

        return consumers;
    }

    private static AsyncEventingBasicConsumer CreateAndRegisterConsumer(
        IModel channel,
        string queueName,
        string routingKey,
        string exchange,
        IDictionary<string, object> arguments,
        Func<IModel, BasicDeliverEventArgs, Task> onMessageReceivedDelegate
    )
    {
        DeclareQueue(channel, queueName, arguments);
        BindQueue(channel, queueName, routingKey, exchange);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArguments) => { await onMessageReceivedDelegate(channel, eventArguments); };

        channel.BasicConsume(queueName, false, consumer);

        return consumer;
    }

    private static void DeclareQueue(
        IModel channel,
        string queueName,
        IDictionary<string, object> arguments
    )
    {
        channel.QueueDeclare(
            queueName,
            true,
            false,
            false,
            arguments
        );
    }

    private static void BindQueue(
        IModel channel,
        string queueName,
        string routingKey,
        string exchange
    )
    {
        channel.QueueBind(
            queueName,
            exchange,
            routingKey
        );
    }
}
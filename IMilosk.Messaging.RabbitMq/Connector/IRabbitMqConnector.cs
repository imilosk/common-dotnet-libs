using RabbitMQ.Client;

namespace IMilosk.Messaging.RabbitMq.Connector;

public interface IRabbitMqConnector
{
    IConnection GetConnection();
}
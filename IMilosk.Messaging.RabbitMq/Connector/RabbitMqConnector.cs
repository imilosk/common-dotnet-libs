using IMilosk.Messaging.RabbitMq.Settings;
using RabbitMQ.Client;

namespace IMilosk.Messaging.RabbitMq.Connector;

public class RabbitMqConnector : IRabbitMqConnector
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqConnector(RabbitMqSettings rabbitMqSettings)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.HostName,
            UserName = rabbitMqSettings.Username,
            Password = rabbitMqSettings.Password,
            Port = rabbitMqSettings.Port,
            VirtualHost = rabbitMqSettings.VirtualHost,
            DispatchConsumersAsync = true
        };
    }

    public IConnection GetConnection()
    {
        return _connectionFactory.CreateConnection();
    }
}
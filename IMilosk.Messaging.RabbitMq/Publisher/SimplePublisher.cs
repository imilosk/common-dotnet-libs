using System.Net.Mime;
using System.Text;
using System.Text.Json;
using IMilosk.Messaging.RabbitMq.Connector;
using IMilosk.Messaging.RabbitMq.Constants;
using IMilosk.Messaging.RabbitMq.Extensions;
using RabbitMQ.Client;

namespace IMilosk.Messaging.RabbitMq.Publisher;

public class SimplePublisher : ISimplePublisher
{
    private readonly IConnection _rabbitMqConnection;
    private readonly IModel _rabbitMqChannel;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SimplePublisher(IRabbitMqConnector rabbitMqConnector)
    {
        _rabbitMqConnection = rabbitMqConnector.GetConnection();
        _rabbitMqChannel = _rabbitMqConnection.CreateModel();
        _rabbitMqChannel.ConfirmSelect();
    }

    public void Dispose()
    {
        _rabbitMqChannel.Dispose();
        _rabbitMqConnection.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Publishes a message to the specified RabbitMQ exchange with the given routing key and user agent.
    /// </summary>
    /// <typeparam name="T">The type of the message to be published.</typeparam>
    /// <param name="messageObj">The message object to be serialized and published.</param>
    /// <param name="exchange">The RabbitMQ exchange to which the message will be published.</param>
    /// <param name="routingKey">The routing key used to route the message to the appropriate queue.</param>
    /// <param name="userAgent">The user agent information to include in the message headers.</param>
    /// <param name="confirmationTimeout">The time (in seconds) to wait for message confirmation. Default is 10 seconds.</param>
    /// <returns>True if the message was acknowledged by the broker within the specified timeout; otherwise, false.</returns>
    public bool PublishJsonMessage<T>(
        T messageObj,
        string exchange,
        string routingKey,
        string userAgent,
        int confirmationTimeout = 10
    )
    {
        var properties = _rabbitMqChannel.CreateBasicProperties();
        properties.Timestamp = DateTime.UtcNow.ToAmqpTimestamp();
        properties.ContentType = MediaTypeNames.Application.Json;
        properties.ContentEncoding = Encoding.UTF8.WebName;
        properties.Persistent = true;
        properties.Headers = new Dictionary<string, object>
        {
            {
                RabbitMqHeaderNames.UserAgent, userAgent
            },
            {
                RabbitMqHeaderNames.ContentType, properties.ContentType
            },
        };

        _rabbitMqChannel.BasicPublish(
            exchange,
            routingKey,
            properties,
            messageObj.ToUtf8JsonReadOnlyMemory(_jsonSerializerOptions)
        );

        return WasMessageAcknowledged(confirmationTimeout);
    }

    private bool WasMessageAcknowledged(int seconds)
    {
        var timeout = TimeSpan.FromSeconds(seconds);

        return _rabbitMqChannel.WaitForConfirms(timeout);
    }
}
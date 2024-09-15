using System.ComponentModel.DataAnnotations;

namespace IMilosk.Messaging.RabbitMq.Settings;

public class RabbitMqSettings
{
    [Required] public string HostName { get; set; } = string.Empty;

    [Required] public string Username { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;

    [Required] public int Port { get; set; }

    [Required] public string VirtualHost { get; set; } = string.Empty;
}
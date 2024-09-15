using System.Buffers;
using System.Text.Json;

namespace IMilosk.Messaging.RabbitMq.Extensions;

public static class ObjectExtensions
{
    public static ReadOnlyMemory<byte> ToUtf8JsonReadOnlyMemory<T>(this T obj, JsonSerializerOptions? options = null)
    {
        var arrayBufferWriter = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(arrayBufferWriter);
        JsonSerializer.Serialize(writer, obj, options);

        return arrayBufferWriter.WrittenMemory;
    }
}
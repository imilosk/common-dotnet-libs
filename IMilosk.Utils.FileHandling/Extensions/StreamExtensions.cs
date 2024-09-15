namespace IMilosk.Utils.FileHandling.Extensions;

public static class StreamExtensions
{
    public static Compression.CompressionType GetCompressionType(this Stream stream)
    {
        return Compression.GetCompressionType(stream);
    }

    public static bool IsCompressed(this Stream stream)
    {
        return Compression.IsCompressed(stream);
    }

    public static Stream Decompress(this Stream stream)
    {
        return Compression.Decompress(stream);
    }

    public static bool TryDecompress(this Stream stream, out Stream decompressedStream)
    {
        return Compression.TryDecompress(stream, out decompressedStream);
    }

    public static Stream DecompressOrDefault(this Stream stream, Stream defaultValue)
    {
        return Compression.DecompressOrDefault(stream, defaultValue);
    }
}
using System.IO.Compression;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.BZip2;

namespace IMilosk.Utils.FileHandling;

public static class Compression
{
    public enum CompressionType
    {
        Unknown = 0,
        GZip = 1,
        Zip = 2,
        BZip2 = 3,
        SevenZip = 4
    }

    public static readonly MemoryStream UnusableStream = new();

    private static readonly byte[] ZipHeader = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] GZipHeader = [0x1F, 0x8B];
    private static readonly byte[] BZip2Header = [0x42, 0x5A, 0x68];
    private static readonly byte[] SevenZipHeader = [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C];

    static Compression()
    {
        // Make sure the UnusableStream is really unusable
        UnusableStream.Dispose();
    }

    /// <summary>
    /// Determines if the given stream is compressed using a supported compression algorithm.
    /// Returns true if the stream is compressed using a known algorithm (GZip, Zip, BZip2, 7Zip).
    /// Returns false if the stream is either not compressed or compressed using an unknown algorithm.
    /// </summary>
    /// <param name="stream">The stream to check for compression.</param>
    /// <returns>True if the stream is compressed using a known algorithm; otherwise, false.</returns>
    /// <remarks>
    /// Assumes that the archive contains only one file.
    /// </remarks>
    internal static bool IsCompressed(Stream stream)
    {
        return GetCompressionType(stream) != CompressionType.Unknown;
    }

    /// <summary>
    /// Decompresses the given stream based on its detected compression type.
    /// Supports GZip, Zip, BZip2, and 7Zip compression formats.
    /// </summary>
    /// <param name="stream">The compressed stream to be decompressed.</param>
    /// <returns>A decompressed stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the compression type is unknown or unsupported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the compression type is outside the supported range.</exception>
    /// <remarks>
    /// This method delegates the decompression to the appropriate method based on the detected compression type.
    /// Assumes that the archive contains only one file.
    /// </remarks>
    internal static Stream Decompress(Stream stream)
    {
        var decompressedStream = ProcessDecompression(stream, UnusableStream);

        if (decompressedStream == UnusableStream)
        {
            throw new InvalidOperationException("Unknown or unsupported compression type.");
        }

        return decompressedStream;
    }

    /// <summary>
    /// Attempts to decompress the given stream based on its detected compression type.
    /// Supports GZip, Zip, BZip2, and 7Zip compression formats.
    /// </summary>
    /// <param name="stream">The compressed stream to be decompressed.</param>
    /// <param name="decompressedStream">The resulting decompressed stream if decompression is successful; otherwise, a bad stream.</param>
    /// <returns>True if the stream was successfully decompressed; otherwise, false.</returns>
    /// <remarks>
    /// This method delegates the decompression to the appropriate method based on the detected compression type.
    /// Assumes that the archive contains only one file.
    /// If the decompression fails or the compression type is unsupported, the <paramref name="decompressedStream"/> will be set to a bad stream.
    /// </remarks>
    internal static bool TryDecompress(Stream stream, out Stream decompressedStream)
    {
        decompressedStream = ProcessDecompression(stream, UnusableStream);

        return decompressedStream != UnusableStream;
    }

    /// <summary>
    /// Attempts to decompress the given stream based on its detected compression type.
    /// Supports GZip, Zip, BZip2, and 7Zip compression formats.
    /// </summary>
    /// <param name="stream">The compressed stream to be decompressed.</param>
    /// <param name="defaultValue">The default stream to return if decompression is unsuccessful.</param>
    /// <returns>The resulting decompressed stream if decompression is successful; otherwise, the default stream.</returns>
    /// <remarks>
    /// This method delegates the decompression to the appropriate method based on the detected compression type.
    /// Assumes that the archive contains only one file.
    /// If the decompression fails or the compression type is unsupported, the method will return the <paramref name="defaultValue"/> stream.
    /// </remarks>
    internal static Stream DecompressOrDefault(Stream stream, Stream defaultValue)
    {
        return ProcessDecompression(stream, defaultValue);
    }

    private static Stream ProcessDecompression(Stream stream, Stream defaultValue)
    {
        var compressionType = GetCompressionType(stream);

        return compressionType switch
        {
            CompressionType.Unknown => defaultValue,
            CompressionType.GZip => DecompressGzip(stream),
            CompressionType.Zip => DecompressZip(stream),
            CompressionType.BZip2 => DecompressBzip2(stream),
            CompressionType.SevenZip => DecompressSevenZip(stream),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static Stream DecompressZip(Stream stream)
    {
        // Assumes that the archive contains only one file
        var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entry = archive.Entries[0];

        return entry.Open();
    }

    private static GZipStream DecompressGzip(Stream stream)
    {
        return new GZipStream(stream, CompressionMode.Decompress);
    }

    private static BZip2Stream DecompressBzip2(Stream stream)
    {
        return new BZip2Stream(stream, SharpCompress.Compressors.CompressionMode.Decompress, false);
    }

    private static Stream DecompressSevenZip(Stream stream)
    {
        // Assumes that the archive contains only one file
        var archive = SevenZipArchive.Open(stream);
        var entry = archive.Entries.First();

        return entry.OpenEntryStream();
    }

    internal static CompressionType GetCompressionType(Stream stream)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            return CompressionType.Unknown;
        }

        const int bufferSize = 6;
        Span<byte> buffer = stackalloc byte[bufferSize];

        if (!ReadStreamHeader(stream, buffer))
        {
            return CompressionType.Unknown;
        }

        return GetCompressionType(buffer);
    }

    private static bool ReadStreamHeader(Stream stream, Span<byte> buffer)
    {
        var originalPosition = stream.Position;
        var bytesRead = stream.Read(buffer);
        stream.Position = originalPosition;

        return bytesRead == buffer.Length;
    }

    private static CompressionType GetCompressionType(ReadOnlySpan<byte> buffer)
    {
        if (buffer.StartsWith(GZipHeader))
        {
            return CompressionType.GZip;
        }

        if (buffer.StartsWith(ZipHeader))
        {
            return CompressionType.Zip;
        }

        if (buffer.StartsWith(BZip2Header))
        {
            return CompressionType.BZip2;
        }

        if (buffer.StartsWith(SevenZipHeader))
        {
            return CompressionType.SevenZip;
        }

        return CompressionType.Unknown;
    }
}
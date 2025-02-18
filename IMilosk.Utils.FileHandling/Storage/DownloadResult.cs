namespace IMilosk.Utils.FileHandling.Storage;

public struct DownloadResult
{
    public static readonly DownloadResult DownloadFailed = new()
    {
        Success = false,
        ObjectMetadata = ObjectMetadata.EmptyObjectMetadata,
    };

    public bool Success;
    public Stream FileStream;
    public ObjectMetadata ObjectMetadata;
}
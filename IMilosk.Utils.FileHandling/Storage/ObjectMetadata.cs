namespace IMilosk.Utils.FileHandling.Storage;

public struct ObjectMetadata
{
    public static readonly ObjectMetadata EmptyObjectMetadata = new()
    {
        Etag = string.Empty,
        ObjectName = string.Empty,
        Size = -1,
        S3VersionId = S3VersionId.Empty,
    };

    public string Etag;
    public string ObjectName;
    public long Size;
    public S3VersionId S3VersionId;
}
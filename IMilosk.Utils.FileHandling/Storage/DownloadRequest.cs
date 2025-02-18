namespace IMilosk.Utils.FileHandling.Storage;

public struct DownloadRequest
{
    public S3Uri S3Uri { get; private set; }
    public S3VersionId S3VersionId { get; private set; }

    public DownloadRequest(S3Uri s3Uri)
    {
        S3Uri = s3Uri;
        S3VersionId = S3VersionId.Empty;
    }

    public DownloadRequest(S3Uri s3Uri, S3VersionId s3VersionId)
    {
        S3Uri = s3Uri;
        S3VersionId = s3VersionId;
    }
}
namespace IMilosk.Utils.FileHandling.Storage;

public interface IBlobStorage
{
    Task<Stream> CreateDownloadStream(string uri);
    Task<Stream> CreateDownloadStream(string uri, S3VersionId s3VersionId);
    Task<Stream> CreateDownloadStream(string bucket, string key);
    Task<Stream> CreateDownloadStream(string bucket, string key, S3VersionId s3VersionId);
    public Task<Stream> CreateDownloadStream(S3Uri s3Uri);
    public Task<Stream> CreateDownloadStream(S3Uri s3Uri, S3VersionId s3VersionId);
    Task<DownloadResult> CreateDownloadStream(DownloadRequest downloadRequest);
    Task<UploadResult> UploadStream(string bucket, string key, string contentType, Stream stream);
    Task<UploadResult> UploadFile(string bucket, string key, string contentType, string filepath);
    Task<SharedFileUriResult> GetSharedFileUri(string bucket, string filepath, string contentType);
    Task<ObjectMetadata> GetObjectMetadata(string uri);
    Task<ObjectMetadata> GetObjectMetadata(string uri, S3VersionId s3VersionId);
    Task<ObjectMetadata> GetObjectMetadata(string bucket, string key);
    Task<ObjectMetadata> GetObjectMetadata(string bucket, string key, S3VersionId s3VersionId);
    public Task<ObjectMetadata> GetObjectMetadata(S3Uri s3Uri);
    public Task<ObjectMetadata> GetObjectMetadata(S3Uri s3Uri, S3VersionId s3VersionId);
    Task CreateBucketIfNotExists(string bucketName);
    Task EnableBucketVersioning(string bucketName);
}
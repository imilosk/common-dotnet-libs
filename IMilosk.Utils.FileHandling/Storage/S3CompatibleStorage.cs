using IMilosk.Utils.FileHandling.Extensions;
using IMilosk.Utils.FileHandling.Settings;
using Minio;
using Minio.DataModel.Args;

namespace IMilosk.Utils.FileHandling.Storage;

public class S3CompatibleStorage : IBlobStorage
{
    private readonly BlobStorageSettings _blobStorageSettings;
    private static readonly Dictionary<string, Dictionary<string, string>> ResponseContentTypeDictionaryCache = [];
    private readonly IMinioClient _minioClient;

    public S3CompatibleStorage(BlobStorageSettings blobStorageSettings)
    {
        _blobStorageSettings = blobStorageSettings;
        _minioClient = new MinioClient()
            .WithEndpoint(blobStorageSettings.Endpoint)
            .WithCredentials(blobStorageSettings.AccessKeyId, blobStorageSettings.SecretAccessKey)
            .WithSSL(blobStorageSettings.UseSsl);

        if (!string.IsNullOrEmpty(blobStorageSettings.Region))
        {
            _minioClient.WithRegion(blobStorageSettings.Region);
        }

        _minioClient.Build();
    }

    public async Task<Stream> CreateDownloadStream(string uri)
    {
        var s3Uri = new S3Uri(uri);
        var downloadRequest = new DownloadRequest(s3Uri);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<Stream> CreateDownloadStream(string uri, S3VersionId s3VersionId)
    {
        var s3Uri = new S3Uri(uri);
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<Stream> CreateDownloadStream(string bucket, string key)
    {
        var s3Uri = new S3Uri(bucket, key);
        var downloadRequest = new DownloadRequest(s3Uri);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<Stream> CreateDownloadStream(string bucket, string key, S3VersionId s3VersionId)
    {
        var s3Uri = new S3Uri(bucket, key);
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<Stream> CreateDownloadStream(S3Uri s3Uri)
    {
        var downloadRequest = new DownloadRequest(s3Uri);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<Stream> CreateDownloadStream(S3Uri s3Uri, S3VersionId s3VersionId)
    {
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return (await CreateDownloadStream(downloadRequest)).FileStream;
    }

    public async Task<DownloadResult> CreateDownloadStream(DownloadRequest downloadRequest)
    {
        var s3RequestBuilder = new S3RequestBuilder(_blobStorageSettings)
            .WithS3Uri(downloadRequest.S3Uri)
            .WithVersionId(downloadRequest.S3VersionId)
            .WithMethod(HttpMethod.Get)
            .SignRequest();

        var request = s3RequestBuilder.CreateHttpRequest();

        var httpClient = _minioClient.Config.HttpClient;
        var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead
        );

        var downloadResponse = response.ToS3DownloadResult(downloadRequest.S3Uri.Key);
        downloadResponse.FileStream = await response.Content.ReadAsStreamAsync();

        return downloadResponse;
    }

    public async Task<ObjectMetadata> GetObjectMetadata(string uri)
    {
        var s3Uri = new S3Uri(uri);
        var downloadRequest = new DownloadRequest(s3Uri);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(string uri, S3VersionId s3VersionId)
    {
        var s3Uri = new S3Uri(uri);
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(string bucket, string key)
    {
        var s3Uri = new S3Uri(bucket, key);
        var downloadRequest = new DownloadRequest(s3Uri);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(string bucket, string key, S3VersionId s3VersionId)
    {
        var s3Uri = new S3Uri(bucket, key);
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(S3Uri s3Uri)
    {
        var downloadRequest = new DownloadRequest(s3Uri);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(S3Uri s3Uri, S3VersionId s3VersionId)
    {
        var downloadRequest = new DownloadRequest(s3Uri, s3VersionId);

        return await GetObjectMetadata(downloadRequest);
    }

    public async Task<ObjectMetadata> GetObjectMetadata(DownloadRequest downloadRequest)
    {
        var getObjectArgs = new StatObjectArgs()
            .WithBucket(downloadRequest.S3Uri.Bucket)
            .WithObject(downloadRequest.S3Uri.Key);

        if (downloadRequest.S3VersionId.IsValid())
        {
            getObjectArgs.WithVersionId(downloadRequest.S3VersionId.ToString());
        }

        var objectStat = await _minioClient.StatObjectAsync(getObjectArgs);

        var objectMetadata = new ObjectMetadata
        {
            Etag = objectStat.ETag,
            ObjectName = objectStat.ObjectName,
            Size = objectStat.Size,
            S3VersionId = objectStat.VersionId,
        };

        return objectMetadata;
    }

    public async Task<UploadResult> UploadStream(
        string bucket,
        string key,
        string contentType,
        Stream stream
    )
    {
        if (!await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(bucket)
            ))
        {
            return UploadResult.UploadFailed;
        }

        var response = await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithContentType(contentType)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
        );

        var uploadSucceeded = response.Etag != "" && response.Size == stream.Length;

        return new UploadResult
        {
            Success = uploadSucceeded,
            Etag = response.Etag,
            ObjectName = response.ObjectName,
            Size = response.Size,
        };
    }

    public async Task<UploadResult> UploadFile(
        string bucket,
        string key,
        string contentType,
        string filepath
    )
    {
        if (!await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(bucket)
            ))
        {
            return UploadResult.UploadFailed;
        }

        var response = await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithContentType(contentType)
                .WithFileName(filepath)
        );

        var uploadSucceeded = response.Etag != "" && response.Size > 0;

        return new UploadResult
        {
            Success = uploadSucceeded,
            Etag = response.Etag,
            ObjectName = response.ObjectName,
            Size = response.Size,
        };
    }

    public async Task<SharedFileUriResult> GetSharedFileUri(
        string bucket,
        string filepath,
        string contentType
    )
    {
        if (!await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(bucket)
            ))
        {
            return SharedFileUriResult.SharedFileUriFailed;
        }

        var reqParams = GetResponseContentTypeDictionary(contentType);

        var presignedUri = await _minioClient.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(filepath)
                .WithExpiry(3600)
                .WithHeaders(reqParams)
        );

        var presigningSucceeded = presignedUri != string.Empty;

        return new SharedFileUriResult
        {
            Success = presigningSucceeded,
            PresignedUri = presignedUri,
        };
    }

    public async Task CreateBucketIfNotExists(string bucketName)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucketName)
        );

        if (!bucketExists)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);

            await _minioClient.MakeBucketAsync(makeBucketArgs);
        }
    }

    public async Task EnableBucketVersioning(string bucketName)
    {
        var args = new SetVersioningArgs().WithBucket(bucketName)
            .WithVersioningEnabled();
        await _minioClient.SetVersioningAsync(args);
    }

    private static Dictionary<string, string> GetResponseContentTypeDictionary(string contentType)
    {
        if (ResponseContentTypeDictionaryCache.TryGetValue(contentType, out var responseContentTypeDictionary))
        {
            return responseContentTypeDictionary;
        }

        responseContentTypeDictionary = new Dictionary<string, string>
        {
            {
                "response-content-type", contentType
            }
        };

        ResponseContentTypeDictionaryCache.Add(contentType, responseContentTypeDictionary);

        return responseContentTypeDictionary;
    }
}
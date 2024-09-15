using IMilosk.Utils.FileHandling.Settings;
using Minio;
using Minio.DataModel.Args;

namespace IMilosk.Utils.FileHandling.Storage;

public class S3CompatibleStorage : IBlobStorage
{
    private static readonly Dictionary<string, Dictionary<string, string>> ResponseContentTypeDictionaryCache = [];
    private readonly IMinioClient _minioClient;

    public S3CompatibleStorage(BlobStorageSettings blobStorageSettings)
    {
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

        return await CreateDownloadStream(s3Uri.Bucket, s3Uri.Key);
    }

    public async Task<Stream> CreateDownloadStream(string bucket, string key)
    {
        var memoryStream = new MemoryStream();

        _ = await _minioClient
            .GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream))
            );

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
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
            Size = response.Size
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
            Size = response.Size
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
using System.Text;
using IMilosk.Tests.Settings;
using IMilosk.Utils.FileHandling.Settings;
using IMilosk.Utils.FileHandling.Storage;

namespace IMilosk.Utils.FileHandling.Tests;

public class S3CompatibleStorageIntegrationTests
{
    private readonly IBlobStorage _storage;
    private static readonly string TestBucket = $"{Guid.NewGuid()}-testing-bucket";
    private const string TestObjectKey = "test-file.txt";
    private const string TestContent = "This is a test file.";
    private const string ContentType = "text/plain";
    private static readonly string StringUri = $"s3://{TestBucket}/{TestObjectKey}";
    private static readonly S3Uri S3Uri = new(StringUri);

    public S3CompatibleStorageIntegrationTests()
    {
        var configuration = ConfigurationHelper.GetDefaultConfiguration();
        var blobStorageSettings = configuration.GetConfigurationOrDefault<BlobStorageSettings>();

        _storage = new S3CompatibleStorage(blobStorageSettings);
    }

    [Fact]
    public async Task CreateDownloadStream_WithBucketAndKey_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream = await _storage.CreateDownloadStream(TestBucket, TestObjectKey);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithBucketAndKeyAndVersion_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream =
            await _storage.CreateDownloadStream(TestBucket, TestObjectKey, metadata.S3VersionId);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithUri_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream = await _storage.CreateDownloadStream(StringUri);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithUriAndVersion_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream = await _storage.CreateDownloadStream(StringUri, metadata.S3VersionId);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithS3Uri_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream = await _storage.CreateDownloadStream(S3Uri);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithS3UriAndVersion_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        await using var downloadStream = await _storage.CreateDownloadStream(S3Uri, metadata.S3VersionId);

        await VerifyResults(metadata, downloadStream, TestObjectKey, TestContent);
    }

    [Fact]
    public async Task CreateDownloadStream_WithDownloadRequest_ShouldSucceed()
    {
        var metadata = await PrepareTest();

        var downloadRequest = new DownloadRequest(S3Uri, metadata.S3VersionId);
        var downloadResult = await _storage.CreateDownloadStream(downloadRequest);

        await VerifyResults(downloadResult, metadata.ObjectName, TestContent, metadata.S3VersionId);
    }

    [Fact]
    public async Task UploadStream_ShouldSucceed()
    {
        _ = await PrepareTest();

        using var stream = GetTestContentMemoryStream();
        var uploadResult = await _storage.UploadStream(TestBucket, TestObjectKey, ContentType, stream);

        VerifyResults(uploadResult, TestObjectKey, TestContent.Length);
    }

    [Fact]
    public async Task GetObjectMetadata_WithBucketAndKey_ShouldReturnValidMetadata()
    {
        await PrepareTest();

        using var stream = GetTestContentMemoryStream();
        var metadata = await _storage.GetObjectMetadata(TestBucket, TestObjectKey);

        VerifyResults(metadata, TestObjectKey, TestContent.Length);
    }

    [Fact]
    public async Task GetObjectMetadata_WithBucketAndKeyAndVersion_ShouldReturnValidMetadata()
    {
        var expectedMetadata = await PrepareTest();

        using var stream = GetTestContentMemoryStream();
        var actualMetadata = await _storage.GetObjectMetadata(TestBucket, TestObjectKey, expectedMetadata.S3VersionId);

        VerifyResults(actualMetadata, expectedMetadata.ObjectName, expectedMetadata.Size, expectedMetadata.S3VersionId);
    }

    [Fact]
    public async Task GetObjectMetadata_WithUri_ShouldReturnMetadata()
    {
        await PrepareTest();

        var metadata = await _storage.GetObjectMetadata(StringUri);

        VerifyResults(metadata, TestObjectKey, TestContent.Length);
    }

    [Fact]
    public async Task GetObjectMetadata_WithUriAndVersion_ShouldReturnMetadata()
    {
        var expectedMetadata = await PrepareTest();

        var actualMetadata = await _storage.GetObjectMetadata(StringUri, expectedMetadata.S3VersionId);

        VerifyResults(actualMetadata, expectedMetadata.ObjectName, expectedMetadata.Size);
    }

    [Fact]
    public async Task GetObjectMetadata_WithS3Uri_ShouldReturnMetadata()
    {
        _ = await PrepareTest();

        var metadata = await _storage.GetObjectMetadata(S3Uri);

        VerifyResults(metadata, TestObjectKey, TestContent.Length);
    }

    [Fact]
    public async Task GetObjectMetadata_WithS3UriAndVersion_ShouldReturnMetadata()
    {
        var expectedMetadata = await PrepareTest();

        var actualMetadata = await _storage.GetObjectMetadata(S3Uri, expectedMetadata.S3VersionId);

        VerifyResults(actualMetadata, expectedMetadata.ObjectName, expectedMetadata.Size, expectedMetadata.S3VersionId);
    }

    [Fact]
    public async Task GetSharedFileUri_ShouldReturnValidUri()
    {
        _ = await PrepareTest();

        var sharedUriResult = await _storage.GetSharedFileUri(TestBucket, TestObjectKey, ContentType);

        VerifyResults(sharedUriResult);
    }

    private async Task<ObjectMetadata> PrepareTest()
    {
        await InitializeBucket();

        return await UploadSampleFile();
    }

    private async Task InitializeBucket()
    {
        await _storage.CreateBucketIfNotExists(TestBucket);
        await _storage.EnableBucketVersioning(TestBucket);
    }

    private async Task<ObjectMetadata> UploadSampleFile()
    {
        using var stream = GetTestContentMemoryStream();
        await _storage.UploadStream(TestBucket, TestObjectKey, ContentType, stream);

        return await _storage.GetObjectMetadata(TestBucket, TestObjectKey);
    }

    private static MemoryStream GetTestContentMemoryStream()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(TestContent));
    }

    private static async Task<string> ReadStreamContent(Stream stream)
    {
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    private static void VerifyResults(
        ObjectMetadata actualMetadata,
        string expectedObjectName,
        long expectedSize,
        string expectedVersionId = ""
    )
    {
        Assert.Equal(expectedObjectName, actualMetadata.ObjectName);
        Assert.NotEmpty(actualMetadata.Etag);
        Assert.Equal(expectedSize, actualMetadata.Size);
        if (!string.IsNullOrEmpty(expectedVersionId))
        {
            Assert.Equal(expectedVersionId, actualMetadata.S3VersionId);
        }
    }

    private static void VerifyResults(
        UploadResult actualResult,
        string expectedObjectName,
        long expectedSize
    )
    {
        Assert.True(actualResult.Success);
        Assert.Equal(expectedObjectName, actualResult.ObjectName);
        Assert.NotEmpty(actualResult.Etag);
        Assert.Equal(expectedSize, actualResult.Size);
    }

    private static async Task VerifyResults(
        DownloadResult actualResult,
        string expectedObjectName,
        string expectedContent,
        string expectedVersionId = ""
    )
    {
        var downloadedContent = await ReadStreamContent(actualResult.FileStream);

        Assert.True(actualResult.Success);
        Assert.Equal(expectedContent, downloadedContent);
        VerifyResults(actualResult.ObjectMetadata, expectedObjectName, downloadedContent.Length, expectedVersionId);
    }

    private static void VerifyResults(SharedFileUriResult actualResult)
    {
        Assert.True(actualResult.Success);
        Assert.StartsWith("http", actualResult.PresignedUri);
    }

    private static async Task VerifyResults(
        ObjectMetadata actualMetadata,
        Stream stream,
        string expectedObjectKey,
        string expectedContent
    )
    {
        var downloadedContent = await ReadStreamContent(stream);

        Assert.Equal(expectedContent, downloadedContent);
        VerifyResults(actualMetadata, expectedObjectKey, expectedContent.Length);
    }
}
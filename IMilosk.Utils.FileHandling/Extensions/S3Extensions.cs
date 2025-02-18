using IMilosk.Utils.FileHandling.Storage;

namespace IMilosk.Utils.FileHandling.Extensions;

static class S3Extensions
{
    public static DownloadResult ToS3DownloadResult(this HttpResponseMessage response, string objectName = "")
    {
        var headers = response.Headers;

        _ = headers.TryGetValues("ETag", out var etagValues);
        _ = headers.TryGetValues("x-amz-version-id", out var s3VersionValues);

        return new DownloadResult
        {
            Success = response.IsSuccessStatusCode,
            ObjectMetadata = new ObjectMetadata
            {
                Etag = etagValues?.FirstOrDefault() ?? string.Empty,
                ObjectName = objectName,
                Size = response.Content.Headers.ContentLength ?? -1,
                S3VersionId = s3VersionValues?.FirstOrDefault() ?? string.Empty,
            },
        };
    }
}
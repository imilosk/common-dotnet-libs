namespace IMilosk.Utils.FileHandling.Storage;

public interface IBlobStorage
{
    Task<Stream> CreateDownloadStream(string uri);
    Task<UploadResult> UploadStream(string bucket, string key, string contentType, Stream stream);
    Task<UploadResult> UploadFile(string bucket, string key, string contentType, string filepath);
    Task<SharedFileUriResult> GetSharedFileUri(string bucket, string filepath, string contentType);
}
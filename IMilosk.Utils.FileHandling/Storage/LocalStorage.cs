namespace IMilosk.Utils.FileHandling.Storage;

public class LocalStorage : IBlobStorage
{
    public Task<Stream> CreateDownloadStream(string uri)
    {
        var stream = new FileStream(uri, FileMode.Open, FileAccess.Read);

        return Task.FromResult<Stream>(stream);
    }

    public Task<UploadResult> UploadStream(string bucket, string key, string contentType, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<UploadResult> UploadFile(string bucket, string key, string contentType, string filepath)
    {
        throw new NotImplementedException();
    }

    public Task<SharedFileUriResult> GetSharedFileUri(string bucket, string filepath, string contentType)
    {
        throw new NotImplementedException();
    }
}
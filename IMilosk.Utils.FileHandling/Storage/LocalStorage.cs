namespace IMilosk.Utils.FileHandling.Storage;

public class LocalStorage
{
    public Task<Stream> CreateDownloadStream(string uri)
    {
        var stream = new FileStream(uri, FileMode.Open, FileAccess.Read);

        return Task.FromResult<Stream>(stream);
    }
}
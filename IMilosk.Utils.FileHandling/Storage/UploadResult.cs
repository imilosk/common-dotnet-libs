namespace IMilosk.Utils.FileHandling.Storage;

public struct UploadResult
{
    public static readonly UploadResult UploadFailed = new() { Success = false, Etag = "", ObjectName = "", Size = -1 };

    public bool Success;
    public string Etag;
    public string ObjectName;
    public long Size;
}
namespace IMilosk.Utils.FileHandling.Storage;

public struct SharedFileUriResult
{
    public static readonly SharedFileUriResult SharedFileUriFailed = new() { Success = false, PresignedUri = "" };
    public bool Success;
    public string PresignedUri;
}
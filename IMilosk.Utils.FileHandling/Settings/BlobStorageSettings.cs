using System.ComponentModel.DataAnnotations;

namespace IMilosk.Utils.FileHandling.Settings;

public class BlobStorageSettings
{
    [Required] public string AccessKeyId { get; init; } = string.Empty;

    [Required] public string SecretAccessKey { get; init; } = string.Empty;

    public string Region { get; init; } = string.Empty;

    [Required] public string Endpoint { get; init; } = string.Empty;

    [Required] public bool UseSsl { get; init; } = false;
}
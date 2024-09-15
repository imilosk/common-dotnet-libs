namespace IMilosk.WebParsingUtils.UnitTests;

public record Article
{
    public long Id { get; set; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public required Uri Link { get; init; }
    public DateTime PublishDate { get; init; }
    public DateTime LastUpdatedTime { get; init; }
    public string Source { get; init; } = string.Empty;
}
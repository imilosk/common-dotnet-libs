namespace IMilosk.WebParsingUtils;

public static class UriConverter
{
    public static Uri ToAbsoluteUrl(Uri baseUrl, string relativeUrl)
    {
        if (Uri.TryCreate(baseUrl, relativeUrl, out var absoluteUri))
        {
            return absoluteUri;
        }

        throw new Exception("Cannot convert to absolute URL");
    }
}
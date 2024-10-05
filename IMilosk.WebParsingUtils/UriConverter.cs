namespace IMilosk.WebParsingUtils;

public static class UriConverter
{
    public static Uri ToAbsoluteUrl(Uri uri, string relativeUrl)
    {
        var baseUri = new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.Host);
        if (Uri.TryCreate(baseUri, relativeUrl, out var absoluteUri))
        {
            return absoluteUri;
        }

        throw new Exception("Cannot convert to absolute URL");
    }
}
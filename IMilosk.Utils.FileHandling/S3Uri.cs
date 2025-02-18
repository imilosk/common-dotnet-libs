using System.Text.RegularExpressions;

namespace IMilosk.Utils.FileHandling;

public partial class S3Uri
{
    // s3://example-bucket/path/to/object
    // https://s3.amazonaws.com/
    // https://s3.amazonaws.com/bucket
    // https://s3.amazonaws.com/bucket/
    // https://s3.amazonaws.com/bucket/key
    // https://s3express.amazonaws.com/
    // https://s3express.amazonaws.com/bucket
    // https://s3express.amazonaws.com/bucket/
    // https://s3express.amazonaws.com/bucket/key
    // http://example-bucket.s3.amazonaws.com/path/to/object
    // https://example-bucket.s3.amazonaws.com/path/to/object

    private const string S3HostRegexPattern = @"^(.+\.)?(?:s3|s3express)[.-]([a-z0-9-]+)\.";

    [GeneratedRegex(S3HostRegexPattern)]
    private static partial Regex S3HostRegexMatch();

    private const string EndpointRegexPattern =
        @"^(?:https?://)(?<bucket>[a-z0-9-]+)\.(?:s3|s3express)?[\.-][a-z0-9-\.]+/(?<key>.+)?$|(?:https?://s3|s3)(?:express)?[\.-][a-z0-9-\.]+/(?<bucket>[^/]+)?/?(?<key>.+)?$";

    [GeneratedRegex(EndpointRegexPattern)]
    private static partial Regex EndpointRegexMatch();

    public string Bucket { get; private set; }

    public string Key { get; private set; }

    public S3Uri(string uri)
    {
        (Bucket, Key) = ParseBucketAndKey(uri);
    }

    public S3Uri(Uri uri)
    {
        (Bucket, Key) = ParseBucketAndKey(uri);
    }

    public S3Uri(string bucket, string key)
    {
        Bucket = bucket;
        Key = key;
    }

    private static (string Bucket, string Key) ParseBucketAndKey(string uri)
    {
        return ParseBucketAndKey(new Uri(uri));
    }

    private static (string Bucket, string Key) ParseBucketAndKey(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (string.IsNullOrEmpty(uri.Host))
            throw new ArgumentException("Invalid URI - no hostname present");

        var uriAbsoluteUri = Uri.UnescapeDataString(uri.AbsoluteUri);
        var uriAbsolutePath = Uri.UnescapeDataString(uri.AbsolutePath);
        if (uriAbsolutePath.StartsWith('/')) uriAbsolutePath = uriAbsolutePath[1..];

        if (uri.Scheme == "s3")
        {
            var bucket = uri.Authority ?? throw new ArgumentException("Invalid S3 URI - no bucket present");
            var key = uriAbsolutePath;

            return (bucket, key);
        }

        if (!S3HostRegexMatch()
                .IsMatch(uri.Host))
            throw new ArgumentException("Invalid S3 URI - hostname does not appear to be a valid S3 endpoint");

        var match = EndpointRegexMatch()
            .Match(uriAbsoluteUri);

        if (match.Success)
        {
            // for host style urls:
            //   group 0 is bucketname plus 's3' prefix and possible region code
            //   group 1 is bucket name
            //   group 2 will be region or 'amazonaws' if US Classic region
            // for path style urls:
            //   group 0 will be s3 prefix plus possible region code
            //   group 1 will be empty
            //   group 2 will be region or 'amazonaws' if US Classic region
            var bucket = match.Groups[1].Value;
            var key = match.Groups[2].Value;

            return (bucket, key);
        }

        throw new ArgumentException("Invalid S3 URI - bucket and key do not appear to be valid");
    }
}
using System.Web;
using IMilosk.Utils.FileHandling.Settings;
using IMilosk.Utils.FileHandling.Utils;

namespace IMilosk.Utils.FileHandling.Storage;

class S3RequestBuilder
{
    private readonly BlobStorageSettings _blobStorageSettings;
    private readonly UriBuilder _uriBuilder;
    private readonly Dictionary<string, string> _headers = new();
    private string _key = string.Empty;
    private string _bucket = string.Empty;
    private HttpMethod HttpMethod { get; set; } = null!;
    private DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public S3RequestBuilder(BlobStorageSettings blobStorageSettings)
    {
        _blobStorageSettings = blobStorageSettings;

        _uriBuilder = new UriBuilder();
        UpdateUri();

        _headers["x-amz-content-sha256"] = "UNSIGNED-PAYLOAD";
        _headers["x-amz-date"] = AwsSigner.ToAmzDateStr(RequestDate);
    }

    private void UpdateUri()
    {
        var isAmazonEndpoint = S3Util.IsAmazonEndpoint(_blobStorageSettings.Endpoint);

        if (isAmazonEndpoint && string.IsNullOrWhiteSpace(_blobStorageSettings.Region))
        {
            throw new ArgumentException("The AWS region is missing.");
        }

        _uriBuilder.Path = isAmazonEndpoint
            ? _key
            : $"{_bucket}/{_key}";

        var hostString = isAmazonEndpoint
            ? $"{_bucket}.s3.amazonaws.com"
            : $"{_blobStorageSettings.Endpoint}";

        _uriBuilder.Host = hostString;

        var parts = hostString.Split(':');
        _uriBuilder.Host = parts[0];

        if (_blobStorageSettings.UseSsl)
        {
            _uriBuilder.Scheme = "https";
            _uriBuilder.Port = 443;
        }
        else
        {
            _uriBuilder.Scheme = "http";
            _uriBuilder.Port = parts.Length > 1 ? int.Parse(parts[1]) : 80;
        }

        _headers["Host"] = _uriBuilder.Host;
    }

    public S3RequestBuilder WithS3Uri(S3Uri s3Uri)
    {
        WithBucket(s3Uri.Bucket);
        WithKey(s3Uri.Key);
        return this;
    }

    public S3RequestBuilder WithBucket(string bucket)
    {
        _bucket = bucket;
        UpdateUri();
        return this;
    }

    public S3RequestBuilder WithKey(string key)
    {
        _key = key;
        UpdateUri();
        return this;
    }

    public S3RequestBuilder WithVersionId(S3VersionId versionId)
    {
        if (!versionId.IsValid())
        {
            return this;
        }

        var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
        query["versionId"] = versionId;
        _uriBuilder.Query = query.ToString();

        return this;
    }

    public S3RequestBuilder WithMethod(HttpMethod method)
    {
        HttpMethod = method;
        return this;
    }

    public S3RequestBuilder WithCustomHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public S3RequestBuilder SignRequest()
    {
        var accessKey = _blobStorageSettings.AccessKeyId;
        var secretKey = _blobStorageSettings.SecretAccessKey;

        var uri = GetUri();
        var requestHeaders = GetHeaders();
        var httpMethod = HttpMethod.Method;

        var path = uri.AbsolutePath;
        var query = GetQueryParameters(uri);
        var region = !string.IsNullOrWhiteSpace(_blobStorageSettings.Region)
            ? _blobStorageSettings.Region
            : "us-east-1";
        var payload = string.Empty;
        const string service = "s3";

        var authorizationHeader = AwsSigner.CreateAuthorizationHeader(
            RequestDate,
            accessKey,
            secretKey,
            requestHeaders,
            query,
            httpMethod,
            path,
            payload,
            region,
            service
        );

        WithCustomHeader("Authorization", authorizationHeader);

        return this;
    }

    private static Dictionary<string, string> GetQueryParameters(Uri uri)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);
        var dictionary = new Dictionary<string, string>();

        foreach (string key in query)
        {
            dictionary[key] = query[key]!;
        }

        return dictionary;
    }

    internal Uri GetUri() => _uriBuilder.Uri;

    private Dictionary<string, string> GetHeaders() => new(_headers);

    public HttpRequestMessage CreateHttpRequest()
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = GetUri()
        };

        foreach (var header in _headers)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return httpRequestMessage;
    }
}
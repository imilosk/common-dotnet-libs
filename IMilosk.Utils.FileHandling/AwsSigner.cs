using System.Security.Cryptography;
using System.Text;

namespace IMilosk.Utils.FileHandling;

public static class AwsSigner
{
    /// <summary>
    /// Create Authorization Header
    /// </summary>
    /// <param name="date"></param>
    /// <param name="accessKey"></param>
    /// <param name="secretKey"></param>
    /// <param name="requestHeaders"></param>
    /// <param name="requestQueryParameters"></param>
    /// <param name="httpMethod"></param>
    /// <param name="path"></param>
    /// <param name="payload"></param>
    /// <param name="region"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public static string CreateAuthorizationHeader(
        DateTime date,
        string accessKey,
        string secretKey,
        Dictionary<string, string> requestHeaders,
        Dictionary<string, string> requestQueryParameters,
        string httpMethod,
        string path,
        string payload,
        string region,
        string service
    )
    {
        var signedHeaders = CreateSignedHeaders(requestHeaders);
        var isUnsigned = IsRequestUnsigned(requestHeaders);

        var canonicalRequest = CreateCanonicalRequest(
            httpMethod,
            path,
            requestHeaders,
            requestQueryParameters,
            payload,
            isUnsigned
        );
        var stringToSign = CreateStringToSign(date, region, service, canonicalRequest);
        var signature = CreateSignature(secretKey, date, region, service, stringToSign);
        var authorizationHeader = CreateAuthorizationHeader(date, accessKey, region, service, signedHeaders, signature);

        return authorizationHeader;
    }

    private static bool IsRequestUnsigned(Dictionary<string, string> requestHeaders)
    {
        _ = requestHeaders.TryGetValue("x-amz-content-sha256", out var amzContentSha256);
        return amzContentSha256 == "UNSIGNED-PAYLOAD";
    }

    private static string CreateAuthorizationHeader(DateTime timestamp, string accessKey, string region, string service,
        string signedHeaders, string signature)
    {
        return
            $"AWS4-HMAC-SHA256 Credential={accessKey}/{CreateCredentialScope(timestamp, region, service)}, SignedHeaders={signedHeaders}, Signature={signature}";
    }


    private static string CreateCanonicalRequest(
        string method,
        string path,
        Dictionary<string, string> headers,
        Dictionary<string, string> query,
        string payload,
        bool isUnsigned
    )
    {
        return string.Join("\n",
            method.ToUpper(),
            path,
            CreateCanonicalQueryString(query),
            CreateCanonicalHeaders(headers),
            CreateSignedHeaders(headers),
            isUnsigned ? "UNSIGNED-PAYLOAD" : HexEncodedHash(payload)
        );
    }

    private static string CreateCanonicalQueryString(Dictionary<string, string> parameters)
    {
        var sortedParams = parameters.OrderBy(p => p.Key);
        return string.Join("&",
            sortedParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    private static string CreateCanonicalHeaders(Dictionary<string, string> headers)
    {
        return string.Concat(
            headers.OrderBy(h => h.Key.ToLowerInvariant())
                .Select(h => $"{h.Key.ToLowerInvariant().Trim()}:{h.Value.Trim()}\n"));
    }

    private static string CreateSignedHeaders(Dictionary<string, string> headers)
    {
        return string.Join(";",
            headers.Keys.Select(x => x.ToLower()
                    .Trim())
                .OrderBy(k => k));
    }

    private static string CreateCredentialScope(DateTime time, string region, string service)
    {
        return string.Join('/', ToDate(time), region, service, "aws4_request");
    }


    private static string CreateStringToSign(DateTime time, string region, string service, string request)
    {
        return string.Join("\n",
            "AWS4-HMAC-SHA256",
            ToAmzDateStr(time),
            CreateCredentialScope(time, region, service),
            HexEncodedHash(request));
    }

    private static string CreateSignature(string secret, DateTime time, string region, string service,
        string stringToSign)
    {
        var h1 = Hmac(Encoding.UTF8.GetBytes("AWS4" + secret), ToDate(time)); // date-key
        var h2 = Hmac(h1, region); // region-key
        var h3 = Hmac(h2, service); // service-key
        var h4 = Hmac(h3, "aws4_request"); // signing-key

        return HmacHex(h4, stringToSign);
    }

    public static string ToAmzDateStr(DateTime time)
    {
        return time.ToString("yyyyMMddTHHmmssZ");
    }

    private static string ToDate(DateTime time)
    {
        return time.ToString("yyyyMMdd");
    }

    private static byte[] Hmac(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);

        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string HmacHex(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);

        return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)))
            .Replace("-", "")
            .ToLower();
    }

    public static string HexEncodedHash(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);

        return HexEncodedHash(bytes);
    }

    public static string HexEncodedHash(byte[] bytes)
    {
        var hashBytes = SHA256.HashData(bytes);

        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLower();
    }
}
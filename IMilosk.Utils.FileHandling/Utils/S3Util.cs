using System.Text.RegularExpressions;

namespace IMilosk.Utils.FileHandling.Utils;

public static class S3Util
{
    internal static bool IsAmazonEndpoint(string endpoint)
    {
        if (IsAmazonChinaEndPoint(endpoint)) return true;
        var rgx = new Regex("^s3[.-]?(.*?)\\.amazonaws\\.com$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture,
            TimeSpan.FromHours(1));
        var matches = rgx.Matches(endpoint);

        return matches.Count > 0;
    }

    internal static bool IsAmazonChinaEndPoint(string endpoint)
    {
        return string.Equals(endpoint, "s3.cn-north-1.amazonaws.com.cn", StringComparison.OrdinalIgnoreCase);
    }
}
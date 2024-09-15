using System.Text.RegularExpressions;

namespace IMilosk.WebParsingUtils;

public static partial class StripHtml
{
    [GeneratedRegex("<.*?>")]
    public static partial Regex StripHtmlTagsRegex();
}
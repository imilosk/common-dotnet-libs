using System.Globalization;

namespace IMilosk.Extensions.BaseTypeExtensions;

public static class SpanExtensions
{
    public static T TryParseOrDefault<T>(this Span<char> input, T defaultValue, CultureInfo cultureInfo)
        where T : ISpanParsable<T>
    {
        return T.TryParse(input, cultureInfo, out var parsedResult) ? parsedResult : defaultValue;
    }
}
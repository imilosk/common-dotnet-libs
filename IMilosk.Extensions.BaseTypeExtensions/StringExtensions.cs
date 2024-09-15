using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace IMilosk.Extensions.BaseTypeExtensions;

public static class StringExtensions
{
    private static readonly Dictionary<string, bool> BooleanMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "yes", true
            },
            {
                "no", false
            },
            {
                "1", true
            },
            {
                "0", false
            }
        };

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string EmptyIfNull(this string? value) => value ?? string.Empty;

    public static T TryParseOrDefault<T>(this string input, T defaultValue, CultureInfo cultureInfo)
        where T : ISpanParsable<T>
    {
        return T.TryParse(input, cultureInfo, out var value0) ? value0 : defaultValue;
    }

    public static bool TryParseOrDefault(this string input, bool defaultValue)
    {
        if (BooleanMapping.TryGetValue(input, out var result))
        {
            return result;
        }

        return bool.TryParse(input, out var parsedResult) ? parsedResult : defaultValue;
    }
}
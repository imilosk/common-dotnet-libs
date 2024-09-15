using System.Globalization;
using System.Xml.Linq;

namespace IMilosk.Extensions.BaseTypeExtensions;

public static class XAttributeExtensions
{
    public static T GetValueOrDefault<T>(
        this XAttribute element,
        T defaultValue,
        CultureInfo cultureInfo
    ) where T : ISpanParsable<T>
    {
        var propertyValue = element.Value;

        return propertyValue.IsNullOrEmpty()
            ? defaultValue
            : propertyValue.TryParseOrDefault(defaultValue, cultureInfo);
    }

    public static bool GetValueOrDefault(
        this XAttribute element,
        bool defaultValue
    )
    {
        var propertyValue = element.Value;

        return propertyValue.IsNullOrEmpty()
            ? defaultValue
            : propertyValue.TryParseOrDefault(defaultValue);
    }
}
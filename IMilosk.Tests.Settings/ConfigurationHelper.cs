using Microsoft.Extensions.Configuration;

namespace IMilosk.Tests.Settings;

public static class ConfigurationHelper
{
    public static IConfiguration GetDefaultConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("secrets.json", true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static T GetConfigurationOrDefault<T>(this IConfiguration configuration) where T : class, new()
    {
        return configuration
            .GetSection(typeof(T).Name)
            .Get<T>() ?? new T();
    }
}
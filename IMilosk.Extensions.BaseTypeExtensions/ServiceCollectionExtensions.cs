using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace IMilosk.Extensions.BaseTypeExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAndValidateServiceOptions<T>(
        this IServiceCollection services,
        IConfigurationRoot configurationRoot) where T : class
    {
        var sectionName = typeof(T).Name;
        var section = configurationRoot.GetSection(sectionName);

        var configValue = section.Get<T>();
        if (configValue is null)
        {
            throw new InvalidOperationException(
                $"Configuration section '{sectionName}' is required for {typeof(T).Name}.");
        }

        services.AddSingleton(configValue);

        services.AddOptions<T>()
            .Configure(options => section
                .Bind(options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection RegisterHttpPolicyHandler<T>(
        this IServiceCollection serviceCollection,
        string httpClientName) where T : class
    {
        serviceCollection.AddHttpClient<T>(httpClientName)
            .AddPolicyHandler((services, _) => HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(
                    Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5),
                    (result, timeSpan, retryCount, _) =>
                    {
                        var logger = services.GetService<ILogger<T>>();
                        logger?.LogWarning(
                            "Retry {RetryCount} encountered an error. Waiting {TimeSpan} before next retry. Exception: {Exception}",
                            retryCount,
                            timeSpan,
                            result.Exception?.Message
                        );
                    }
                )
            );

        return serviceCollection;
    }
}
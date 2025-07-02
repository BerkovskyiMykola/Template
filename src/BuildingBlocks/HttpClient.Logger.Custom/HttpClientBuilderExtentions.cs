using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Extension methods for configuring custom logging in HttpClient.
/// </summary>
public static class HttpClientBuilderExtentions
{
    /// <summary>
    /// Configures the HttpClient to add a custom logger.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure.</param>
    /// <param name="configure">An action to configure <see cref="HttpClientLoggerHandlerOptions"/>.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    /// <exception cref="InvalidOperationException">Thrown when the resulting <see cref="IHttpClientBuilder"/> is <c>null</c>.</exception>
    public static IHttpClientBuilder AddCustomLogger(
        this IHttpClientBuilder builder,
        Action<HttpClientLoggerHandlerOptions> configure)
    {
        builder.Services.Configure(builder.Name, configure);

        builder.AddHttpMessageHandler(sp =>
        {
            var optionsFactory = sp.GetRequiredService<IOptionsFactory<HttpClientLoggerHandlerOptions>>();
            var timeProvider = sp.GetRequiredService<TimeProvider>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.{nameof(HttpClientLoggerHandler)}");
            return new HttpClientLoggerHandler(optionsFactory.Create(builder.Name), timeProvider, logger);
        });

        return builder;
    }
}

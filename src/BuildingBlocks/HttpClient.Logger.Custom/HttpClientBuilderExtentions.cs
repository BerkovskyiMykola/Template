using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Extension methods for configuring logging in <see cref="System.Net.Http.HttpClient"/>.
/// </summary>
public static class HttpClientBuilderExtentions
{
    /// <summary>
    /// Adds a duration logger handler to the <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a duration logger handler will be added.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a duration logger handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>  
    /// <exception cref="InvalidOperationException">Thrown when the resulting <see cref="IHttpClientBuilder"/> is <c>null</c>.</exception>
    public static IHttpClientBuilder AddDurationLoggerHandler(
        this IHttpClientBuilder builder)
    {
        builder.AddHttpMessageHandler(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.{nameof(DurationLoggerHandler)}");
            return new DurationLoggerHandler.Handler(logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds a request to send logger handler to the <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a request to send logger handler will be added.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a request to send logger handler.</returns>
    /// <returns>The configured <see cref="IHttpClientBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    /// <exception cref="InvalidOperationException">Thrown when the resulting <see cref="IHttpClientBuilder"/> is <c>null</c>.</exception>
    public static IHttpClientBuilder AddRequestToSendLoggerHandler(
        this IHttpClientBuilder builder,
        Action<RequestToSendLoggerHandler.Options> configure)
    {
        builder.Services.Configure(builder.Name, configure);

        builder.AddHttpMessageHandler(sp =>
        {
            var optionsFactory = sp.GetRequiredService<IOptionsFactory<RequestToSendLoggerHandler.Options>>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.{nameof(RequestToSendLoggerHandler)}");
            return new RequestToSendLoggerHandler.Handler(optionsFactory.Create(builder.Name), logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds a response logger handler to the <see cref="System.Net.Http.HttpClient"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a response logger handler will be added.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a response logger handler.</returns>
    /// <returns>The configured <see cref="IHttpClientBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    /// <exception cref="InvalidOperationException">Thrown when the resulting <see cref="IHttpClientBuilder"/> is <c>null</c>.</exception>
    public static IHttpClientBuilder AddResponseLoggerHandler(
        this IHttpClientBuilder builder,
        Action<ResponseLoggerHandler.Options> configure)
    {
        builder.Services.Configure(builder.Name, configure);

        builder.AddHttpMessageHandler(sp =>
        {
            var optionsFactory = sp.GetRequiredService<IOptionsFactory<ResponseLoggerHandler.Options>>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.{nameof(ResponseLoggerHandler)}");
            return new ResponseLoggerHandler.Handler(optionsFactory.Create(builder.Name), logger);
        });

        return builder;
    }
}

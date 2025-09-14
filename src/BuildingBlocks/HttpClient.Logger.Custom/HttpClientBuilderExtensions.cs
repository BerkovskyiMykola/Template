/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Extension methods for configuring logging in <see cref="System.Net.Http.HttpClient"/>.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a duration logger handler to the <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a duration logger handler will be added.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a duration logger handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>  
    public static IHttpClientBuilder AddDurationLoggerHandler(
        this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddHttpMessageHandler(sp =>
        {
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.DurationLoggerHandler");
            return new DurationHandler.Handler(logger);
        });
    }

    /// <summary>
    /// Adds a request to send logger handler to the <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a request to send logger handler will be added.</param>
    /// <param name="configure">The <see cref="Action{ResponseHandler.HandlerOptions}"/> to configure <see cref="ResponseHandler.HandlerOptions"/> of a request to send logger handler.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a request to send logger handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    public static IHttpClientBuilder AddRequestToSendLoggerHandler(
        this IHttpClientBuilder builder,
        Action<RequestToSendHandler.HandlerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Services.Configure(builder.Name, configure);

        return builder.AddHttpMessageHandler(sp =>
        {
            IOptionsFactory<RequestToSendHandler.HandlerOptions> optionsFactory = sp.GetRequiredService<IOptionsFactory<RequestToSendHandler.HandlerOptions>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.RequestToSendLoggerHandler");
            return new RequestToSendHandler.Handler(optionsFactory.Create(builder.Name), logger);
        });
    }

    /// <summary>
    /// Adds a response logger handler to the <see cref="System.Net.Http.HttpClient"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to which a response logger handler will be added.</param>
    /// <param name="configure">The <see cref="Action{ResponseHandler.HandlerOptions}"/> to configure <see cref="ResponseHandler.HandlerOptions"/> of a response logger handler.</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/> that includes a response logger handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    public static IHttpClientBuilder AddResponseLoggerHandler(
        this IHttpClientBuilder builder,
        Action<ResponseHandler.HandlerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        _ = builder.Services.Configure(builder.Name, configure);

        return builder.AddHttpMessageHandler(sp =>
        {
            IOptionsFactory<ResponseHandler.HandlerOptions> optionsFactory = sp.GetRequiredService<IOptionsFactory<ResponseHandler.HandlerOptions>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.ResponseLoggerHandler");
            return new ResponseHandler.Handler(optionsFactory.Create(builder.Name), logger);
        });
    }
}

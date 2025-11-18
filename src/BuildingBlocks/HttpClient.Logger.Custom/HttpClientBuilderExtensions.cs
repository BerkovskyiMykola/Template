/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
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
    /// <param name="builder">The builder to which a duration logger handler will be added.</param>
    /// <returns>The original builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>  
    public static IHttpClientBuilder AddDurationLoggerHandler(
        this IHttpClientBuilder builder)
    {
        Guard.IsNotNull(builder);
        
        return builder.AddHttpMessageHandler(sp =>
        {
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(
                $"System.Net.Http.HttpClient.{builder.Name}.DurationLoggerHandler");

            return new DurationHandler.Handler(logger);
        });
    }

    /// <summary>
    /// Adds a request to send logger handler to the <see cref="System.Net.Http.HttpClient"/>
    /// </summary>
    /// <param name="builder">The builder to which a request to send logger handler will be added.</param>
    /// <param name="configure">The action to configure options of a request to send logger handler.</param>
    /// <returns>The original builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    public static IHttpClientBuilder AddRequestToSendLoggerHandler(
        this IHttpClientBuilder builder,
        Action<RequestToSendHandler.HandlerOptions> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        _ = builder.Services.Configure(builder.Name, configure);
        _ = builder.Services.AddPooled<PooledStringNullableObjectPairList>();

        return builder.AddHttpMessageHandler(sp =>
        {
            IOptionsFactory<RequestToSendHandler.HandlerOptions> optionsFactory 
                = sp.GetRequiredService<IOptionsFactory<RequestToSendHandler.HandlerOptions>>();
            ObjectPool<PooledStringNullableObjectPairList> objectPool 
                = sp.GetRequiredService<ObjectPool<PooledStringNullableObjectPairList>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(
                $"System.Net.Http.HttpClient.{builder.Name}.RequestToSendLoggerHandler");

            return new RequestToSendHandler.Handler(
                optionsFactory.Create(builder.Name), 
                objectPool, 
                logger);
        });
    }

    /// <summary>
    /// Adds a response logger handler to the <see cref="System.Net.Http.HttpClient"/>.
    /// </summary>
    /// <param name="builder">The builder to which a response logger handler will be added.</param>
    /// <param name="configure">The action to configure options of a response logger handler.</param>
    /// <returns>The original builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>  
    public static IHttpClientBuilder AddResponseLoggerHandler(
        this IHttpClientBuilder builder,
        Action<ResponseHandler.HandlerOptions> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        _ = builder.Services.Configure(builder.Name, configure);
        _ = builder.Services.AddPooled<PooledStringNullableObjectPairList>();

        return builder.AddHttpMessageHandler(sp =>
        {
            IOptionsFactory<ResponseHandler.HandlerOptions> optionsFactory 
                = sp.GetRequiredService<IOptionsFactory<ResponseHandler.HandlerOptions>>();
            ObjectPool<PooledStringNullableObjectPairList> objectPool 
                = sp.GetRequiredService<ObjectPool<PooledStringNullableObjectPairList>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(
                $"System.Net.Http.HttpClient.{builder.Name}.ResponseLoggerHandler");

            return new ResponseHandler.Handler(
                optionsFactory.Create(builder.Name), 
                objectPool, 
                logger);
        });
    }
}

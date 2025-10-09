/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using HttpClient.Logger.Custom;
using Microsoft.Extensions.Configuration;

namespace Template.Api.Common.HttpClients;

/// <summary>  
/// Provides extension methods for configuring and adding <see cref="System.Net.Http.HttpClient"/> services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// The name of the <see cref="System.Net.Http.HttpClient"/> instance used in <see cref="AddConfiguredTestTraceNamedHttpClient"/>.
    /// </summary>
    internal const string TestTraceNamedHttpClient = "TestTrace";

    /// <summary>  
    /// Adds and configures <see cref="System.Net.Http.HttpClient"/> service, using the <see cref="TestTraceNamedHttpClient"/> name, 
    /// based on the <paramref name="configuration"/>, to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to which the configured <see cref="System.Net.Http.HttpClient"/> service will be added.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing configuration settings for the <see cref="System.Net.Http.HttpClient"/> service.</param>  
    /// <returns>The <see cref="IServiceCollection"/> instance with <see cref="System.Net.Http.HttpClient"/> service configured.</returns>  
    internal static IServiceCollection AddConfiguredTestTraceNamedHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IHttpClientBuilder builder = services.AddHttpClient(TestTraceNamedHttpClient)
            .RemoveAllLoggers();

        IConfigurationSection httpClientLoggingSection = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging");

        bool enableDurationLogger = httpClientLoggingSection.GetValue<bool>("EnableDuration");

        if (enableDurationLogger)
        {
            _ = builder.AddDurationLoggerHandler();
        }
        
        HttpClient.Logger.Custom.RequestToSendHandler.LoggingFields requestLoggingFields = httpClientLoggingSection
            .GetValue<HttpClient.Logger.Custom.RequestToSendHandler.LoggingFields>("RequestLoggingFields");

        HttpClient.Logger.Custom.ResponseHandler.LoggingFields responseLoggingFields = httpClientLoggingSection
            .GetValue<HttpClient.Logger.Custom.ResponseHandler.LoggingFields>("ResponseLoggingFields");

        string[] allowedRequestHeaders = httpClientLoggingSection.GetSection("AllowedRequestHeaders").Get<string[]>() ?? [];
        string[] allowedResponseHeaders = httpClientLoggingSection.GetSection("AllowedResponseHeaders").Get<string[]>() ?? [];
        TextMediaTypeOptions[] allowedTextMediaTypes = httpClientLoggingSection.GetSection("AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];

        int requestBodyLogLimit = httpClientLoggingSection.GetValue<int>("RequestBodyLogLimit");
        int responseBodyLogLimit = httpClientLoggingSection.GetValue<int>("ResponseBodyLogLimit");

        _ = builder
            .AddResponseLoggerHandler(config =>
            {
                config.LoggingFields = responseLoggingFields;

                foreach (string header in allowedResponseHeaders)
                {
                    _ = config.AllowedHeaders.Add(header);
                }

                foreach (TextMediaTypeOptions textMediaType in allowedTextMediaTypes)
                {
                    config.AllowedMediaTypes.AddText(textMediaType.ContentType, Encoding.GetEncoding(textMediaType.Encoding));
                }

                config.BodyLogLimit = responseBodyLogLimit;
            })
            .AddRequestToSendLoggerHandler(config =>
            {
                config.LoggingFields = requestLoggingFields;

                foreach (string header in allowedRequestHeaders) 
                { 
                    _ = config.AllowedHeaders.Add(header); 
                }

                foreach (TextMediaTypeOptions textMediaType in allowedTextMediaTypes)
                {
                    config.AllowedMediaTypes.AddText(textMediaType.ContentType, Encoding.GetEncoding(textMediaType.Encoding));
                }

                config.BodyLogLimit = requestBodyLogLimit;
            })
            .AddStandardResilienceHandler();

        return services;
    }

    private sealed record TextMediaTypeOptions(string ContentType, string Encoding);
}

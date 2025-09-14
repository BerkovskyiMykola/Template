/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using Microsoft.AspNetCore.HttpLogging;

namespace Template.Api.Common.HttpLogging;

/// <summary>  
/// Provides extension methods for configuring and adding HTTP logging services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds and configures HTTP logging services, based on the <paramref name="configuration"/>, to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configured HTTP logging to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing HTTP logging settings.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with HTTP logging configured.</returns>  
    internal static IServiceCollection AddConfiguredHttpLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddHttpLogging(config =>
        {
            HttpLoggingFields loggingFields = configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
            string[] allowedRequestHeaders = configuration.GetSection("HttpLogging:AllowedRequestHeaders").Get<string[]>() ?? [];
            string[] allowedResponseHeaders = configuration.GetSection("HttpLogging:AllowedResponseHeaders").Get<string[]>() ?? [];
            TextMediaTypeOptions[] allowedTextMediaTypes = configuration.GetSection("HttpLogging:AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];
            int requestBodyLogLimit = configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
            int responseBodyLogLimit = configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

            config.LoggingFields = loggingFields;
            
            config.RequestHeaders.Clear();
            foreach (string header in allowedRequestHeaders)
            {
                _ = config.RequestHeaders.Add(header);
            }

            config.ResponseHeaders.Clear();
            foreach (string header in allowedResponseHeaders)
            {
                _ = config.ResponseHeaders.Add(header);
            }

            config.MediaTypeOptions.Clear();
            foreach (TextMediaTypeOptions textMediaType in allowedTextMediaTypes)
            {
                config.MediaTypeOptions.AddText(textMediaType.ContentType, Encoding.GetEncoding(textMediaType.Encoding));
            }

            config.RequestBodyLogLimit = requestBodyLogLimit;
            config.ResponseBodyLogLimit = responseBodyLogLimit;
        });
    }

    private sealed record TextMediaTypeOptions(string ContentType, string Encoding);
}

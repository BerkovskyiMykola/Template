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
    /// Adds and configures HTTP logging services, based on the provided <paramref name="configuration"/>, to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to which HTTP logging services will be added.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> instance containing HTTP logging settings.</param>  
    /// <returns>The <see cref="IServiceCollection"/> instance with HTTP logging configured.</returns>  
    internal static IServiceCollection AddConfiguredHttpLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection httpLoggingSection = configuration.GetSection("HttpLogging");

        HttpLoggingFields loggingFields = httpLoggingSection.GetValue<HttpLoggingFields>("LoggingFields");
        string[] allowedRequestHeaders = httpLoggingSection.GetSection("AllowedRequestHeaders").Get<string[]>() ?? [];
        string[] allowedResponseHeaders = httpLoggingSection.GetSection("AllowedResponseHeaders").Get<string[]>() ?? [];
        TextMediaTypeOptions[] allowedTextMediaTypes = httpLoggingSection.GetSection("AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];
        int requestBodyLogLimit = httpLoggingSection.GetValue<int>("RequestBodyLogLimit");
        int responseBodyLogLimit = httpLoggingSection.GetValue<int>("ResponseBodyLogLimit");

        return services.AddHttpLogging(config =>
        {
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

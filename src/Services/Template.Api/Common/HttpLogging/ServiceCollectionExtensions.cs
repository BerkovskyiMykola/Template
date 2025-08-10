using System.Text;
using Microsoft.AspNetCore.HttpLogging;

namespace Template.Api.Common.HttpLogging;

/// <summary>  
/// Provides extension methods for configuring and adding HTTP logging services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds and configures HTTP logging services based on the provided configuration to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configured HTTP logging to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing HTTP logging settings.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with HTTP logging configured.</returns>  
    public static IServiceCollection AddConfiguredHttpLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpLogging(config =>
        {
            var loggingFields = configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
            var allowedRequestHeaders = configuration.GetSection("HttpLogging:AllowedRequestHeaders").Get<string[]>() ?? [];
            var allowedResponseHeaders = configuration.GetSection("HttpLogging:AllowedResponseHeaders").Get<string[]>() ?? [];
            var allowedTextMediaTypes = configuration.GetSection("HttpLogging:AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];
            var requestBodyLogLimit = configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
            var responseBodyLogLimit = configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

            config.LoggingFields = loggingFields;

            config.RequestHeaders.Clear();
            foreach (var header in allowedRequestHeaders) config.RequestHeaders.Add(header);

            config.ResponseHeaders.Clear();
            foreach (var header in allowedResponseHeaders) config.ResponseHeaders.Add(header);

            config.MediaTypeOptions.Clear();
            foreach (var textContentType in allowedTextMediaTypes) config.MediaTypeOptions.AddText(textContentType.ContentType, Encoding.GetEncoding(textContentType.Encoding));

            config.RequestBodyLogLimit = requestBodyLogLimit;
            config.ResponseBodyLogLimit = responseBodyLogLimit;
        });

        return services;
    }

    /// <summary>
    /// Represents options for a text-based media type, including its content type and encoding.
    /// </summary>
    private sealed record TextMediaTypeOptions
    {
        /// <summary>
        /// Gets the content type of the text media (e.g., "application/json", "text/plain").
        /// </summary>
        public required string ContentType { get; init; }

        /// <summary>
        /// Gets the encoding name used for the text media (e.g., "utf-8", "ascii").
        /// </summary>
        public required string Encoding { get; init; }
    }
}

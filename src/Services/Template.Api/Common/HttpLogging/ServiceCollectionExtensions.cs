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
    public static IServiceCollection AddConfiguredHttpLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddHttpLogging(config =>
        {
            HttpLoggingFields loggingFields = configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
            var allowedRequestHeaders = configuration.GetSection("HttpLogging:AllowedRequestHeaders").Get<string[]>() ?? [];
            var allowedResponseHeaders = configuration.GetSection("HttpLogging:AllowedResponseHeaders").Get<string[]>() ?? [];
            TextMediaTypeOptions[] allowedTextMediaTypes = configuration.GetSection("HttpLogging:AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];
            var requestBodyLogLimit = configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
            var responseBodyLogLimit = configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

            config.LoggingFields = loggingFields;
            
            config.RequestHeaders.Clear();
            foreach (var header in allowedRequestHeaders)
            {
                _ = config.RequestHeaders.Add(header);
            }

            config.ResponseHeaders.Clear();
            foreach (var header in allowedResponseHeaders)
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

    /// <summary>
    /// Represents options for a text-based media type, including its content type and encoding.
    /// </summary>
    /// <param name="ContentType">The content type of the text media (e.g., "application/json", "text/plain").</param>
    /// <param name="Encoding">The encoding name used for the text media (e.g., "utf-8", "ascii").</param>
    private sealed record TextMediaTypeOptions(string ContentType, string Encoding);
}

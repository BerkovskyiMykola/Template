using Microsoft.AspNetCore.HttpLogging;
using System.Text;

namespace Template.Api.Common.HttpLogging;

/// <summary>  
/// Provides extension methods for HTTP logging in the application.  
/// </summary>  
internal static class HttpLoggingExtensions
{
    /// <summary>  
    /// Adds and configures HTTP logging services based on the provided configuration.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add the HTTP logging to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing HTTP logging settings.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with HTTP logging configured.</returns>  
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>  
    public static IServiceCollection AddConfiguredHttpLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpLogging(config =>
        {
            var loggingFields = configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
            var requestHeaders = configuration.GetSection("HttpLogging:RequestHeaders").Get<string[]>() ?? [];
            var responseHeaders = configuration.GetSection("HttpLogging:ResponseHeaders").Get<string[]>() ?? [];
            var textMediaTypes = configuration.GetSection("HttpLogging:TextContentTypes").Get<TextContentTypeOption[]>() ?? [];
            var requestBodyLogLimit = configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
            var responseBodyLogLimit = configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

            config.LoggingFields = loggingFields;

            config.RequestHeaders.Clear();
            foreach (var header in requestHeaders) config.RequestHeaders.Add(header);

            config.ResponseHeaders.Clear();
            foreach (var header in responseHeaders) config.ResponseHeaders.Add(header);

            config.MediaTypeOptions.Clear();
            foreach (var textMediaType in textMediaTypes) config.MediaTypeOptions.AddText(textMediaType.MediaType, Encoding.GetEncoding(textMediaType.Encoding));

            config.RequestBodyLogLimit = requestBodyLogLimit;
            config.ResponseBodyLogLimit = responseBodyLogLimit;
        });

        return services;
    }

    private sealed record TextContentTypeOption
    {
        public string MediaType { get; init; } = string.Empty;
        public string Encoding { get; init; } = string.Empty;
    }
}

using Microsoft.AspNetCore.HttpLogging;
using System.Text;

namespace Template.Api.Common.HttpLogging;

/// <summary>  
/// Provides extension methods for configuring and adding HTTP logging services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
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
            var textContentTypes = configuration.GetSection("HttpLogging:TextContentTypes").Get<TextContentTypeOptions[]>() ?? [];
            var requestBodyLogLimit = configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
            var responseBodyLogLimit = configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

            config.LoggingFields = loggingFields;

            config.RequestHeaders.Clear();
            foreach (var header in requestHeaders) config.RequestHeaders.Add(header);

            config.ResponseHeaders.Clear();
            foreach (var header in responseHeaders) config.ResponseHeaders.Add(header);

            config.MediaTypeOptions.Clear();
            foreach (var textContentType in textContentTypes) config.MediaTypeOptions.AddText(textContentType.MediaType, Encoding.GetEncoding(textContentType.Encoding));

            config.RequestBodyLogLimit = requestBodyLogLimit;
            config.ResponseBodyLogLimit = responseBodyLogLimit;
        });

        return services;
    }

    private sealed record TextContentTypeOptions
    {
        public string MediaType { get; init; } = string.Empty;
        public string Encoding { get; init; } = string.Empty;
    }
}

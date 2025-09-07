using System.Text;
using HttpClient.Logger.Custom;

namespace Template.Api.Common.HttpClients;

/// <summary>  
/// Provides extension methods for configuring and adding <see cref="System.Net.Http.HttpClient"/> services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// The name of the <see cref="System.Net.Http.HttpClient"/> instance used in <see cref="AddConfiguredTestTraceNamedHttpClient"/>.
    /// </summary>
    public const string TestTraceNamedHttpClient = "TestTrace";

    /// <summary>  
    /// Adds and configures <see cref="System.Net.Http.HttpClient"/> service, using the <see cref="TestTraceNamedHttpClient"/> name, based on the <paramref name="configuration"/>, to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configured <see cref="System.Net.Http.HttpClient"/> service to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing <see cref="System.Net.Http.HttpClient"/> service settings.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with <see cref="System.Net.Http.HttpClient"/> service configured.</returns>  
    public static IServiceCollection AddConfiguredTestTraceNamedHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IHttpClientBuilder builder = services.AddHttpClient(TestTraceNamedHttpClient)
            .RemoveAllLoggers();

        var enableDurationLogger = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:EnableDuration").Get<bool>();

        if (enableDurationLogger)
        {
            _ = builder.AddDurationLoggerHandler();
        }

        HttpClient.Logger.Custom.RequestToSendHandler.LoggingFields requestLoggingFields = configuration
            .GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:RequestLoggingFields")
            .Get<HttpClient.Logger.Custom.RequestToSendHandler.LoggingFields>();
        HttpClient.Logger.Custom.ResponseHandler.LoggingFields responseLoggingFields = configuration
            .GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:ResponseLoggingFields")
            .Get<HttpClient.Logger.Custom.ResponseHandler.LoggingFields>();
        var allowedRequestHeaders = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:AllowedRequestHeaders").Get<string[]>() ?? [];
        var allowedResponseHeaders = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:AllowedResponseHeaders").Get<string[]>() ?? [];
        TextMediaTypeOptions[] allowedTextMediaTypes = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:AllowedTextMediaTypes").Get<TextMediaTypeOptions[]>() ?? [];
        var requestBodyLogLimit = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:RequestBodyLogLimit").Get<int>();
        var responseBodyLogLimit = configuration.GetSection($"HttpClients:{TestTraceNamedHttpClient}:Logging:ResponseBodyLogLimit").Get<int>();

        _ = builder
            .AddResponseLoggerHandler(config =>
            {
                config.LoggingFields = responseLoggingFields;

                foreach (var header in allowedResponseHeaders)
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

                foreach (var header in allowedRequestHeaders) 
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

    /// <summary>
    /// Represents options for a text-based media type, including its content type and encoding.
    /// </summary>
    /// <param name="ContentType">The content type of the text media (e.g., "application/json", "text/plain").</param>
    /// <param name="Encoding">The encoding name used for the text media (e.g., "utf-8", "ascii").</param>
    private sealed record TextMediaTypeOptions(string ContentType, string Encoding);
}

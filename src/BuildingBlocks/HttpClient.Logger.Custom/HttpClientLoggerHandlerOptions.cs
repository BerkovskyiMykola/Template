using Microsoft.Net.Http.Headers;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Options for the <see cref="HttpClientLoggerHandler"/>.
/// </summary>
public sealed record HttpClientLoggerHandlerOptions
{
    /// <summary>
    /// Fields to log for the Request and Response.
    /// </summary>
    public HttpClientLoggingFields LoggingFields { get; set; } = HttpClientLoggingFields.None;

    /// <summary>
    /// Request header values that are allowed to be logged.
    /// <para>
    /// If a request header is not present in the <see cref="RequestHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// Request headers can contain authentication tokens,
    /// or private information which may have regulatory concerns
    /// under GDPR and other laws. Arbitrary request headers
    /// should not be logged unless logs are secure and
    /// access controlled and the privacy impact assessed.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when trying to set a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public ISet<string> RequestHeaders { get; set; } = new HashSet<string>();

    /// <summary>
    /// Response header values that are allowed to be logged.
    /// <para>
    /// If a response header is not present in the <see cref="ResponseHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when trying to set a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public ISet<string> ResponseHeaders { get; set; } = new HashSet<string>();

    /// <summary>
    /// Options for configuring encodings for a specific media type.
    /// <para>
    /// If the request or response do not match the supported media type,
    /// the response body will not be logged.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when trying to set a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public IList<MediaTypeHeaderValue> TextContentTypes { get; set; } = [];

    /// <summary>
    /// Maximum request body size to log (in bytes).
    /// </summary>
    public int RequestBodyLogLimit { get; set; } = 0;

    /// <summary>
    /// Maximum response body size to log (in bytes).
    /// </summary>
    public int ResponseBodyLogLimit { get; set; } = 0;
}

using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Provides extension methods for logging HTTP client request and response information.
/// </summary>
internal static partial class LoggingExtensions
{
    /// <summary>
    /// Logs HTTP request to send information using a <see cref="HttpRequestToSendLog"/> object.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="log">The HTTP request to send log details.</param>
    public static void LogInformationRequestLogToSend(this ILogger logger, HttpRequestToSendLog log) => logger.Log(
        LogLevel.Information,
        new EventId(1, "HttpClientRequestLogToSend"),
        log,
        exception: null,
        formatter: HttpLog.Callback);

    /// <summary>
    /// Logs HTTP response information using a <see cref="HttpResponseLog"/> object.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="log">The HTTP response log details.</param>
    public static void LogInformationResponseLog(this ILogger logger, HttpResponseLog log) => logger.Log(
        LogLevel.Information,
        new EventId(2, "HttpClientResponseLog"),
        log,
        exception: null,
        formatter: HttpLog.Callback);

    /// <summary>
    /// Logs the HTTP request body to send and a status message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="body">The request body content.</param>
    [LoggerMessage(3, LogLevel.Information, "RequestBody to send: {Body}", EventName = "HttpClientRequestBodyToSend")]
    public static partial void LogInformationRequestBodyToSend(this ILogger logger, string body);

    /// <summary>
    /// Logs the HTTP response body.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="body">The response body content.</param>
    [LoggerMessage(4, LogLevel.Information, "ResponseBody: {Body}", EventName = "HttpClientResponseBody")]
    public static partial void LogInformationResponseBody(this ILogger logger, string body);

    /// <summary>
    /// Logs a decode failure when converting the response body.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(5, LogLevel.Debug, "Decode failure while converting body", EventName = "HttpClientDecodeFailure")]
    public static partial void LogDebugDecodeFailure(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs a warning for unrecognized media type in request to send.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(6, LogLevel.Debug, "Unrecognized Content-Type for request body to send", EventName = "HttpClientUnrecognizedRequestToSendMediaType")]
    public static partial void LogDebugUnrecognizedRequestToSendMediaType(this ILogger logger);

    /// <summary>
    /// Logs a warning for unrecognized media type in response.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(7, LogLevel.Debug, "Unrecognized Content-Type for response body", EventName = "HttpClientUnrecognizedResponseMediaType")]
    public static partial void LogDebugUnrecognizedResponseMediaType(this ILogger logger);

    /// <summary>
    /// Logs a warning when no media type header is found for request to send.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(8, LogLevel.Debug, "No Content-Type header for request body to send", EventName = "HttpClientRequestToSendNoMediaType")]
    public static partial void LogDebugRequestToSendNoMediaType(this ILogger logger);

    /// <summary>
    /// Logs a warning when no media type header is found for response.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(9, LogLevel.Debug, "No Content-Type header for response body", EventName = "HttpClientResponseNoMediaType")]
    public static partial void LogDebugResponseNoMediaType(this ILogger logger);

    /// <summary>
    /// Logs the duration of the HTTP request/response lifecycle in milliseconds.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="duration">The duration in milliseconds.</param>
    [LoggerMessage(10, LogLevel.Information, "Duration: {Duration}ms", EventName = "HttpClientDuration")]
    public static partial void LogInformationDuration(this ILogger logger, double duration);
}

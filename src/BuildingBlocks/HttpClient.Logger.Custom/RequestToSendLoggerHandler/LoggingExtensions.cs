using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.RequestToSendLoggerHandler;

/// <summary>
/// Provides extension methods for logging <see cref="HttpRequestMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    public static void LogRequestToSendLogAsInformation(this ILogger logger, IReadOnlyList<KeyValuePair<string, object?>> log) => logger.Log(
        LogLevel.Information,
        new EventId(1, "HttpClientRequestToSendLog"),
        log,
        exception: null,
        formatter: (log, exception) => Helper.FormatLog("Request to send", log));

    [LoggerMessage(2, LogLevel.Information, "RequestBody to send: {Body}", EventName = "HttpClientRequestBodyToSend")]
    public static partial void LogRequestBodyToSendAsInformation(this ILogger logger, string body);

    [LoggerMessage(3, LogLevel.Debug, "Unrecognized Content-Type for request body to send", EventName = "HttpClientUnrecognizedRequestToSendMediaType")]
    public static partial void LogUnrecognizedRequestToSendMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(4, LogLevel.Debug, "No Content-Type header for request body to send", EventName = "HttpClientRequestToSendNoMediaType")]
    public static partial void LogRequestToSendNoMediaTypeAsDebug(this ILogger logger);
}

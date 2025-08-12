using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// Provides extension methods for logging <see cref="HttpResponseMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    public static void LogResponseLogAsInformation(this ILogger logger, IReadOnlyList<KeyValuePair<string, object?>> log) => logger.Log(
        LogLevel.Information,
        new EventId(1, "HttpClientResponseLog"),
        log,
        exception: null,
        formatter: (log, exception) => Helper.FormatLog("Response", log));

    [LoggerMessage(2, LogLevel.Information, "ResponseBody: {Body}", EventName = "HttpClientResponseBody")]
    public static partial void LogResponseBodyAsInformation(this ILogger logger, string body);

    [LoggerMessage(3, LogLevel.Debug, "Unrecognized Content-Type for response body", EventName = "HttpClientUnrecognizedResponseMediaType")]
    public static partial void LogUnrecognizedResponseMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(4, LogLevel.Debug, "No Content-Type header for response body", EventName = "HttpClientResponseNoMediaType")]
    public static partial void LogResponseNoMediaTypeAsDebug(this ILogger logger);
}

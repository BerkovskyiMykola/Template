/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.ResponseHandler;

#pragma warning disable S109 

/// <summary>
/// Provides extension methods for logging <see cref="HttpResponseMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    internal static void LogResponseLogAsInformation(this ILogger logger, IReadOnlyList<LogField> log) 
        => logger.Log(
            LogLevel.Information,
            new EventId(3001, "HttpClientResponseLog"),
            log,
            exception: null,
            formatter: (log, _) => Helper.FormatLog("Response", log));

    [LoggerMessage(3002, LogLevel.Information, "ResponseBody: {Body}", EventName = "HttpClientResponseBody")]
    internal static partial void LogResponseBodyAsInformation(this ILogger logger, string body);

    [LoggerMessage(3003, LogLevel.Debug, "Unrecognized Content-Type for response body", EventName = "HttpClientUnrecognizedResponseMediaType")]
    internal static partial void LogUnrecognizedResponseMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(3004, LogLevel.Debug, "No Content-Type header for response body", EventName = "HttpClientResponseNoMediaType")]
    internal static partial void LogResponseNoMediaTypeAsDebug(this ILogger logger);
}

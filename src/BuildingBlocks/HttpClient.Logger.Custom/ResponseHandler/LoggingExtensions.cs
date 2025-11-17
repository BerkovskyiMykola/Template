/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;
using LogField = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom.ResponseHandler;

#pragma warning disable S109 

/// <summary>
/// Provides extension methods for logging <see cref="HttpResponseMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    public static void LogResponseLogAsInformation(this ILogger logger, IReadOnlyList<LogField> log) 
        => logger.Log(
            LogLevel.Information,
            new EventId(3001, "HttpClientResponseLog"),
            new HttpLog("Response", log),
            exception: null,
            formatter: static (state, _) => state.ToString());

    [LoggerMessage(3002, LogLevel.Information, "ResponseBody: {Body}", EventName = "HttpClientResponseBody")]
    public static partial void LogResponseBodyAsInformation(this ILogger logger, string body);

    [LoggerMessage(3003, LogLevel.Debug, "Unrecognized Content-Type for response body", EventName = "HttpClientUnrecognizedResponseMediaType")]
    public static partial void LogUnrecognizedResponseMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(3004, LogLevel.Debug, "No Content-Type header for response body", EventName = "HttpClientResponseNoMediaType")]
    public static partial void LogResponseNoMediaTypeAsDebug(this ILogger logger);
}

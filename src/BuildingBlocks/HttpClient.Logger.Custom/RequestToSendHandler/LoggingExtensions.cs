/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.RequestToSendHandler;

#pragma warning disable S109 

/// <summary>
/// Provides extension methods for logging <see cref="HttpRequestMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    internal static void LogRequestToSendLogAsInformation(this ILogger logger, IReadOnlyList<LogField> log) 
        => logger.Log(
            LogLevel.Information,
            new EventId(2001, "HttpClientRequestToSendLog"),
            log,
            exception: null,
            formatter: (log, _) => Helper.FormatLog("Request to send", log));

    [LoggerMessage(2002, LogLevel.Information, "RequestBody to send: {Body}", EventName = "HttpClientRequestBodyToSend")]
    internal static partial void LogRequestBodyToSendAsInformation(this ILogger logger, string body);

    [LoggerMessage(2003, LogLevel.Debug, "Unrecognized Content-Type for request body to send", EventName = "HttpClientUnrecognizedRequestToSendMediaType")]
    internal static partial void LogUnrecognizedRequestToSendMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(2004, LogLevel.Debug, "No Content-Type header for request body to send", EventName = "HttpClientRequestToSendNoMediaType")]
    internal static partial void LogRequestToSendNoMediaTypeAsDebug(this ILogger logger);
}

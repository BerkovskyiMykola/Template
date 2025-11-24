/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom.RequestToSendHandler;

#pragma warning disable S109 

/// <summary>
/// Extension methods for logging <see cref="HttpRequestMessage"/>.
/// </summary>
internal static partial class LoggingExtensions
{
    public static void LogRequestToSendLogAsInformation(
        this ILogger logger, 
        IReadOnlyList<StringNullableObjectPair> keyValuePairs)
    {
        logger.Log(
            LogLevel.Information,
            new EventId(2001, "HttpClientRequestToSendLog"),
            new HttpLog("Request to send", keyValuePairs),
            exception: null,
            formatter: static (state, _) => state.ToString());
    }

    [LoggerMessage(
        2002, 
        LogLevel.Information, 
        "RequestBody to send: {Body}", 
        EventName = "HttpClientRequestBodyToSend")]
    public static partial void LogRequestBodyToSendAsInformation(this ILogger logger, string body);

    [LoggerMessage(
        2003, 
        LogLevel.Debug, 
        "Unrecognized Content-Type for request body to send", 
        EventName = "HttpClientUnrecognizedRequestToSendMediaType")]
    public static partial void LogUnrecognizedRequestToSendMediaTypeAsDebug(this ILogger logger);

    [LoggerMessage(
        2004, 
        LogLevel.Debug, 
        "No Content-Type header for request body to send", 
        EventName = "HttpClientRequestToSendNoMediaType")]
    public static partial void LogRequestToSendNoMediaTypeAsDebug(this ILogger logger);
}

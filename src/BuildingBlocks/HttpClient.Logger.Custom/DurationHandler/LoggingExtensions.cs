/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.DurationHandler;

#pragma warning disable S109 

/// <summary>
/// Provides extension methods for logging <see cref="System.Net.Http.HttpClient"/> operation duration information.
/// </summary>
internal static partial class LoggingExtensions
{
    [LoggerMessage(1001, LogLevel.Information, "Duration: {durationMilliseconds}ms", EventName = "HttpClientDurationMilliseconds")]
    internal static partial void LogDurationAsInformation(this ILogger logger, long durationMilliseconds);
}

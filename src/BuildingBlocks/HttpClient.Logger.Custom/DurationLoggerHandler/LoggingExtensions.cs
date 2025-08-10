using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.DurationLoggerHandler;

/// <summary>
/// Provides extension methods for logging <see cref="System.Net.Http.HttpClient"/> operation duration information.
/// </summary>
internal static partial class LoggingExtensions
{
    [LoggerMessage(1, LogLevel.Information, "Duration: {durationMilliseconds}ms", EventName = "HttpClientDurationMilliseconds")]
    public static partial void LogDurationAsInformation(this ILogger logger, double durationMilliseconds);
}

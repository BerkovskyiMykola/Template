using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Provides extension methods for logging <see cref="System.Net.Http.HttpClient"/> related diagnostics and errors.
/// </summary>
internal static partial class LoggingExtensions
{
    [LoggerMessage(1, LogLevel.Debug, "Decode failure while converting body", EventName = "HttpClientBodyDecodeFailure")]
    public static partial void LogBodyDecodeFailureAsDebug(this ILogger logger, Exception ex);
}

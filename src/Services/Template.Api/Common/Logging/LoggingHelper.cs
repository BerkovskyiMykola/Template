namespace Template.Api.Common.Logging;

/// <summary>
/// Provides helper methods for logging.
/// </summary>
internal static partial class LoggingHelper
{
    /// <summary>
    /// Logs an informational message with the specified context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="something">The contextual information to log.</param>
    [LoggerMessage(1000, LogLevel.Information, "Something is logged: {Something}")]
    public static partial void LogInformationSomething(this ILogger logger, string something);

    /// <summary>
    /// Logs an error message with the specified context and exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="something">The contextual information to log.</param>
    /// <param name="ex">The exception to include in the log.</param>
    [LoggerMessage(1001, LogLevel.Error, "Something is logged: {Something}")]
    public static partial void LogErrorSomething(this ILogger logger, string something, Exception ex);
}

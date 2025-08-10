namespace Template.Api.Common.Logging;

/// <summary>
/// Provides extension methods for logging.
/// </summary>
internal static partial class LoggingExtensions
{
    [LoggerMessage(1000, LogLevel.Information, "Something is logged: {Something}")]
    public static partial void LogInformationSomething(this ILogger logger, string something);

    [LoggerMessage(1001, LogLevel.Error, "Something is logged: {Something}")]
    public static partial void LogErrorSomething(this ILogger logger, string something, Exception ex);
}

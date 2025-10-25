/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Template.Api.Workers;

/// <summary>
/// Provides extension methods for logging in Workers.
/// </summary>
internal static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Information, "Worker running")]
    public static partial void LogWorkerRunningAsInformation(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Worker is stopping")]
    public static partial void LogWorkerStoppingAsInformation(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "An unhandled exception occurred while processing the current worker iteration")]
    public static partial void LogWorkerIterationFailedAsError(this ILogger logger, Exception ex);
}

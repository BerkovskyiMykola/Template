/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Logging.File.Custom;

/// <summary>
/// Describes the file logger behavior when the queue becomes full.
/// </summary>
public enum LoggerQueueFullMode
{
    /// <summary>
    /// Blocks the logging threads once the queue limit is reached.
    /// </summary>
    Wait,
    /// <summary>
    /// Drops new log messages when the queue is full.
    /// </summary>
    DropWrite
}

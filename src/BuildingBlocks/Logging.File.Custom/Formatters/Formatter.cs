/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Logging.File.Custom.Formatters;

/// <summary>
/// Allows custom log messages formatting.
/// </summary>
/// <param name="name">The name of the formatter.</param>
internal abstract class Formatter(string name)
{
    /// <summary>
    /// Gets the name associated with the file log formatter.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Writes the log message to the specified TextWriter.
    /// </summary>
    /// <param name="logEntry">The log entry.</param>
    /// <param name="scopeProvider">The provider of scope data.</param>
    /// <param name="textWriter">The string writer.</param>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    public abstract void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter);
}

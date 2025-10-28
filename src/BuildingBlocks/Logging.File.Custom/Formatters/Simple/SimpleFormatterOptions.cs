/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Logging.File.Custom.Formatters.Simple;

/// <summary>
/// Options for the Simple file log formatter.
/// </summary>
public sealed class SimpleFormatterOptions : FormatterOptions
{
    /// <summary>
    /// Gets or sets a value that indicates whether the entire message is logged in a single line.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the entire message is logged in a single line.
    /// </value>
    public bool SingleLine { get; set; }
}

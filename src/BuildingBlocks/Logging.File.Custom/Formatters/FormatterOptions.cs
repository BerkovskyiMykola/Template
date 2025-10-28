/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics.CodeAnalysis;

namespace Logging.File.Custom.Formatters;

/// <summary>
/// Options for the file log formatter.
/// </summary>
public class FormatterOptions
{
    /// <summary>
    /// Gets or sets a value that indicates whether scopes are included.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if scopes are included.
    /// </value>
    public bool IncludeScopes { get; set; }

    /// <summary>
    /// Gets or sets the format string used to format timestamp in logging messages.
    /// </summary>
    /// <value>
    /// The default is <see langword="null" />.
    /// </value>
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    public string? TimestampFormat { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether or not UTC timezone should be used to format timestamps in logging messages.
    /// </summary>
    /// <value>
    /// The default is <see langword="false" />.
    /// </value>
    public bool UseUtcTimestamp { get; set; }
}

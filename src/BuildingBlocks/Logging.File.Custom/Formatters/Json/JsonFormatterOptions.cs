/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text.Json;

namespace Logging.File.Custom.Formatters.Json;

/// <summary>
/// Options for the JSON file log formatter.
/// </summary>
public sealed class JsonFormatterOptions : FormatterOptions
{
    /// <summary>
    /// Gets or sets <see cref="System.Text.Json.JsonWriterOptions"/>.
    /// </summary>
    public JsonWriterOptions JsonWriterOptions { get; set; }
}

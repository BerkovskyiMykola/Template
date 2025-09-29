/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace HttpClient.Logger.Custom;

/// <summary>
/// Represents a log field with a key and an associated value.
/// </summary>
/// <param name="Key">The name of the log field.</param>
/// <param name="Value">The value associated with the log field.</param>
internal sealed record LogField(
    string Key,
    object? Value);

/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics.Extensions;

namespace HttpClient.Logger.Custom.ResponseHandler;

#pragma warning disable S2325

/// <summary>
/// Options for the <see cref="Handler"/>.
/// </summary>
public sealed class HandlerOptions
{
    /// <summary>
    /// Fields to log for the <see cref="HttpRequestMessage"/>.
    /// Defaults to <see cref="LoggingFields.None"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the value assigned is not a valid <see cref="ResponseHandler.LoggingFields"/> enum.
    /// </exception>
    public LoggingFields LoggingFields
    {
        get;
        set
        {
            GuardExt.IsDefinedFlagsEnumCombination(value);

            field = value;
        }
    } = LoggingFields.None;

    /// <summary>
    /// <see cref="HttpResponseMessage.Headers"/> that are allowed to be logged.
    /// <para>
    /// If a header is not present in the <see cref="AllowedHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// </para>
    /// </summary>
    public HashSet<string> AllowedHeaders { get; } = [];

    /// <summary>
    /// Options for configuring encodings for a specific <see cref="HttpResponseMessage.Content"/> media type.
    /// <para>
    /// If the <see cref="HttpResponseMessage.Content"/> does not match the supported media type,
    /// the <see cref="HttpResponseMessage.Content"/> will not be logged.
    /// </para>
    /// </summary>
    public MediaTypeOptions AllowedMediaTypes { get; } = new();

    /// <summary>
    /// Maximum <see cref="HttpResponseMessage.Content"/> size to log (in bytes).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if a negative value is assigned.
    /// </exception>
    public int BodyLogLimit
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            field = value;
        }
    }
}

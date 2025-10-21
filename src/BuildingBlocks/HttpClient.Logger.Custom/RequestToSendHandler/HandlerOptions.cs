﻿/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace HttpClient.Logger.Custom.RequestToSendHandler;

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
    /// Thrown if the value assigned is not a valid <see cref="RequestToSendHandler.LoggingFields"/> enum.
    /// </exception>
    public LoggingFields LoggingFields
    { 
        get;
        set
        {
            if (!Helper.IsFlaggedEnumValid(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"{nameof(value)} ('{value}') must be a valid {nameof(RequestToSendHandler.LoggingFields)}.");
            }

            field = value;
        } 
    } = LoggingFields.None;

    /// <summary>
    /// <see cref="HttpRequestMessage.Headers"/> that are allowed to be logged.
    /// <para>
    /// If a header is not present in the <see cref="AllowedHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// <see cref="HttpRequestMessage.Headers"/> can contain authentication tokens,
    /// or private information which may have regulatory concerns
    /// under GDPR and other laws. Arbitrary <see cref="HttpRequestMessage.Headers"/>
    /// should not be logged unless logs are secure and
    /// access controlled and the privacy impact assessed.
    /// </para>
    /// </summary>
    public HashSet<string> AllowedHeaders { get; } = [];

    /// <summary>
    /// Options for configuring encodings for a specific <see cref="HttpRequestMessage.Content"/> media type.
    /// <para>
    /// If the <see cref="HttpRequestMessage.Content"/> does not match the supported media type,
    /// the <see cref="HttpRequestMessage.Content"/> will not be logged.
    /// </para>
    /// </summary>
    public MediaTypeOptions AllowedMediaTypes { get; } = new();

    /// <summary>
    /// Maximum <see cref="HttpRequestMessage.Content"/> size to log (in bytes).
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

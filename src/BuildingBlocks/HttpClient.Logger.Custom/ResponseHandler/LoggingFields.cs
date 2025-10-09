/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// Flags used to control which parts of the
/// <see cref="HttpResponseMessage"/> are logged.
/// </summary>
[Flags]
public enum LoggingFields
{
    /// <summary>
    /// No logging.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flag for logging the <see cref="HttpResponseMessage.StatusCode"/>.
    /// <para>
    /// For example:
    /// StatusCode: 200
    /// </para>
    /// </summary>
    StatusCode = 1,

    /// <summary>
    /// Flag for logging the <see cref="HttpResponseMessage.Headers"/>.
    /// <para>
    /// <see cref="HttpResponseMessage.Headers"/> are redacted by default with the character '[Redacted]' unless specified in
    /// the <see cref="HandlerOptions.AllowedHeaders"/>.
    /// </para>
    /// <para>
    /// For example:
    /// Content-Length: 16
    /// My-Custom-Response-Header: [Redacted]
    /// </para>
    /// </summary>
    Headers = 2,

    /// <summary>
    /// Flag for logging the <see cref="HttpResponseMessage.Content"/>.
    /// Logging the <see cref="HttpResponseMessage.Content"/> up to <see cref="HandlerOptions.BodyLogLimit"/>.
    /// </summary>
    Body = 4,

    /// <summary>
    /// Flag for logging <see cref="HttpResponseMessage"/> properties and headers.
    /// Includes <see cref="StatusCode"/> and <see cref="Headers"/>.
    /// </summary>
    PropertiesAndHeaders = StatusCode | Headers,

    /// <summary>
    /// Flag for logging the entire <see cref="HttpResponseMessage"/>.
    /// Includes <see cref="PropertiesAndHeaders"/> and <see cref="Body"/>.
    /// </summary>
    All = PropertiesAndHeaders | Body
}

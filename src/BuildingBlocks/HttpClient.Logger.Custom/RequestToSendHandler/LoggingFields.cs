/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace HttpClient.Logger.Custom.RequestToSendHandler;

/// <summary>
/// Flags used to control which parts of the
/// <see cref="HttpRequestMessage"/> are logged.
/// </summary>
[Flags]
public enum LoggingFields
{
    /// <summary>
    /// No logging.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.Version"/>.
    /// <para>
    /// For example:
    /// Protocol: HTTP/1.1
    /// </para>
    /// </summary>
    Protocol = 1,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.Method"/>.
    /// <para>
    /// For example:
    /// Method: GET
    /// </para>
    /// </summary>
    Method = 2,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.RequestUri"/> <c>Scheme</c> (See <see cref="Uri.Scheme"/>).
    /// <para>
    /// For example:
    /// Scheme: https
    /// </para>
    /// </summary>
    Scheme = 4,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.RequestUri"/> <c>Host</c> <c>Port</c> (See <see cref="Uri.Host"/>, <see cref="Uri.Port"/>).
    /// <para>
    /// For example:
    /// Host: example.com:443
    /// </para>
    /// </summary>
    Host = 8,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.RequestUri"/> <c>AbsolutePath</c> (See <see cref="Uri.AbsolutePath"/>).
    /// <para>
    /// For example:
    /// AbsolutePath: /api/users/123
    /// </para>
    /// </summary>
    AbsolutePath = 16,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.RequestUri"/> <c>Query</c> (See <see cref="Uri.Query"/>).
    /// <para>
    /// For example:
    /// Query: ?includeDetails=true
    /// </para>
    /// </summary>
    Query = 32,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.Headers"/>.
    /// <see cref="HttpRequestMessage.Headers"/> are redacted by default with the character '[Redacted]' unless specified in
    /// the <see cref="HandlerOptions.AllowedHeaders"/>.
    /// <para>
    /// For example:
    /// Connection: keep-alive
    /// My-Custom-Request-Header: [Redacted]
    /// </para>
    /// </summary>
    Headers = 64,

    /// <summary>
    /// Flag for logging the <see cref="HttpRequestMessage.Content"/>.
    /// Logging the <see cref="HttpRequestMessage.Content"/> up to <see cref="HandlerOptions.BodyLogLimit"/>.
    /// </summary>
    Body = 128,

    /// <summary>
    /// Flag for logging a collection of <see cref="HttpRequestMessage"/> properties,
    /// including <see cref="Protocol"/>, <see cref="Method"/>,
    /// <see cref="Scheme"/>, <see cref="Host"/>, and <see cref="AbsolutePath"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpRequestMessage.RequestUri"/> <c>Query</c> (See <see cref="Uri.Query"/>) is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="Query"/>.
    /// </remarks>
    Properties = Protocol | Method | Scheme | Host | AbsolutePath,

    /// <summary>
    /// Flag for logging <see cref="HttpRequestMessage"/> properties and headers.
    /// Includes <see cref="Properties"/> and <see cref="Headers"/>
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpRequestMessage.RequestUri"/> <c>Query</c> (See <see cref="Uri.Query"/>) is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="Query"/>.
    /// </remarks>
    PropertiesAndHeaders = Properties | Headers,

    /// <summary>
    /// Flag for logging the entire <see cref="HttpRequestMessage"/>.
    /// Includes <see cref="PropertiesAndHeaders"/> and <see cref="Body"/>.
    /// Logging the <see cref="HttpRequestMessage.Content"/> up to <see cref="HandlerOptions.BodyLogLimit"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpRequestMessage.RequestUri"/> <c>Query</c> (See <see cref="Uri.Query"/>) is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="Query"/>.
    /// </remarks>
    All = PropertiesAndHeaders | Body
}

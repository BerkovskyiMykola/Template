namespace HttpClient.Logger.Custom;

/// <summary>
/// Flags used to control which parts of the
/// request and response are logged.
/// </summary>
[Flags]
public enum LoggingFields : long
{
    /// <summary>
    /// No logging.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.Version"/>.
    /// <para>
    /// For example:
    /// Protocol: HTTP/1.1
    /// </para>
    /// </summary>
    RequestProtocol = 1,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.Method"/>.
    /// <para>
    /// For example:
    /// Method: GET
    /// </para>
    /// </summary>
    RequestMethod = 2,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.RequestUri.Scheme"/>.
    /// <para>
    /// For example:
    /// Scheme: https
    /// </para>
    /// </summary>
    RequestScheme = 4,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.RequestUri.Host"/> and <see cref="HttpRequestMessage.RequestUri.Port"/>.
    /// <para>
    /// For example:
    /// Host: example.com:443
    /// </para>
    /// </summary>
    RequestHost = 8,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.RequestUri.AbsolutePath"/>.
    /// <para>
    /// For example:
    /// AbsolutePath: /api/users/123
    /// </para>
    /// </summary>
    RequestAbsolutePath = 16,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.RequestUri.Query"/>.
    /// <para>
    /// For example:
    /// Query: ?includeDetails=true
    /// </para>
    /// </summary>
    RequestQuery = 32,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.Content.Headers"/>.
    /// Headers are redacted by default with the character '[Redacted]' unless specified in
    /// the <see cref="LoggerHandlerOptions.RequestHeaders"/>.
    /// <para>
    /// For example:
    /// Connection: keep-alive
    /// My-Custom-Request-Header: [Redacted]
    /// </para>
    /// </summary>
    RequestHeaders = 64,

    /// <summary>
    /// Flag for logging the HTTP Request <see cref="HttpRequestMessage.Content"/>.
    /// Logging the request body up to <see cref="LoggerHandlerOptions.RequestBodyLogLimit"/>.
    /// </summary>
    RequestBody = 128,

    /// <summary>
    /// Flag for logging the HTTP Response <see cref="HttpResponseMessage.StatusCode"/>.
    /// <para>
    /// For example:
    /// StatusCode: 200
    /// </para>
    /// </summary>
    ResponseStatusCode = 256,

    /// <summary>
    /// Flag for logging the HTTP Response <see cref="HttpResponseMessage.Content.Headers"/>.
    /// <para>
    /// Headers are redacted by default with the character '[Redacted]' unless specified in
    /// the <see cref="LoggerHandlerOptions.ResponseHeaders"/>.
    /// </para>
    /// <para>
    /// For example:
    /// Content-Length: 16
    /// My-Custom-Response-Header: [Redacted]
    /// </para>
    /// </summary>
    ResponseHeaders = 512,

    /// <summary>
    /// Flag for logging the HTTP Response <see cref="HttpResponseMessage.Content"/>.
    /// Logging the response body up to <see cref="LoggerHandlerOptions.ResponseBodyLogLimit"/>.
    /// </summary>
    ResponseBody = 1024,

    /// <summary>
    /// Flag for logging how long it took to process the request and response in milliseconds.
    /// </summary>
    Duration = 2048,

    /// <summary>
    /// Flag for logging a collection of HTTP Request properties,
    /// including <see cref="RequestProtocol"/>, <see cref="RequestMethod"/>,
    /// <see cref="RequestScheme"/>, <see cref="RequestHost"/>, and <see cref="RequestAbsolutePath"/>.
    /// </summary>
    RequestProperties = RequestProtocol | RequestMethod | RequestScheme | RequestHost | RequestAbsolutePath,

    /// <summary>
    /// Flag for logging HTTP Request properties and headers.
    /// Includes <see cref="RequestProperties"/> and <see cref="RequestHeaders"/>
    /// </summary>
    /// <remarks>
    /// The HTTP Request <see cref="HttpRequestMessage.RequestUri.Query"/> is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="RequestQuery"/>.
    /// </remarks>
    RequestPropertiesAndHeaders = RequestProperties | RequestHeaders,

    /// <summary>
    /// Flag for logging HTTP Response properties and headers.
    /// Includes <see cref="ResponseStatusCode"/> and <see cref="ResponseHeaders"/>.
    /// </summary>
    ResponsePropertiesAndHeaders = ResponseStatusCode | ResponseHeaders,

    /// <summary>
    /// Flag for logging the entire HTTP Request.
    /// Includes <see cref="RequestPropertiesAndHeaders"/> and <see cref="RequestBody"/>.
    /// Logging the request body up to <see cref="LoggerHandlerOptions.RequestBodyLogLimit"/>.
    /// </summary>
    /// <remarks>
    /// The HTTP Request <see cref="HttpRequestMessage.RequestUri.Query"/> is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="RequestQuery"/>.
    /// </remarks>
    Request = RequestPropertiesAndHeaders | RequestBody,

    /// <summary>
    /// Flag for logging the entire HTTP Response.
    /// Includes <see cref="ResponsePropertiesAndHeaders"/> and <see cref="ResponseBody"/>.
    /// Logging the request body up to <see cref="LoggerHandlerOptions.ResponseBodyLogLimit"/>.
    /// </summary>
    Response = ResponsePropertiesAndHeaders | ResponseBody,

    /// <summary>
    /// Flag for logging both the HTTP Request and Response.
    /// Includes <see cref="Request"/>, <see cref="Response"/>, and <see cref="Duration"/>.
    /// Logging the request and response body up to the <see cref="LoggerHandlerOptions.RequestBodyLogLimit"/>
    /// and <see cref="LoggerHandlerOptions.ResponseBodyLogLimit"/>.
    /// </summary>
    /// <remarks>
    /// The HTTP Request <see cref="HttpRequestMessage.RequestUri.Query"/> is not included with this flag as it may contain private information.
    /// If desired, it should be explicitly specified with <see cref="RequestQuery"/>.
    /// </remarks>
    All = Request | Response | Duration
}

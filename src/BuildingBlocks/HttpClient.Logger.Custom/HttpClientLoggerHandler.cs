using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs HTTP request and response details
/// according to configured logging options.
/// </summary>
internal sealed class HttpClientLoggerHandler : DelegatingHandler
{
    private readonly HttpClientLoggerHandlerOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private const string Redacted = "[Redacted]";

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientLoggerHandler"/> class.
    /// </summary>
    /// <param name="options">The options controlling which parts of the request/response are logged.</param>
    /// <param name="timeProvider">The time provider used to measure request duration.</param>
    /// <param name="logger">The logger used for logging messages.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/>, <paramref name="timeProvider"/>, or <paramref name="logger"/> is null.</exception>  
    public HttpClientLoggerHandler(
        HttpClientLoggerHandlerOptions options,
        TimeProvider timeProvider,
        ILogger logger)
    {
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously and logs the request and response based on the configured options.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>  
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogLevel.Information) || _options.LoggingFields == HttpClientLoggingFields.None)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var startTimestamp = _timeProvider.GetTimestamp();

        LogRequestPropertiesAndHeaders(request);

        await LogRequestBodyAsync(request, cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        LogResponsePropertiesAndHeaders(response);

        await LogResponseBodyAsync(response, cancellationToken);

        LogDuration(startTimestamp);

        return response;
    }

    /// <summary>
    /// Logs the HTTP request properties and headers based on the configured logging fields.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>  
    private void LogRequestPropertiesAndHeaders(HttpRequestMessage request)
    {
        List<KeyValuePair<string, object?>>? parameters = null;

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestProtocol))
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new("Protocol", $"HTTP/{request.Version}"));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestMethod))
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(request.Method), request.Method));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestScheme)
            && request.RequestUri is not null
            && request.RequestUri.IsAbsoluteUri)
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(request.RequestUri.Scheme), request.RequestUri.Scheme));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestHost)
            && request.RequestUri is not null
            && request.RequestUri.IsAbsoluteUri)
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(request.RequestUri.Host), request.RequestUri.Host));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestAbsolutePath)
            && request.RequestUri is not null
            && request.RequestUri.IsAbsoluteUri)
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(request.RequestUri.AbsolutePath), request.RequestUri.AbsolutePath));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestQuery)
            && request.RequestUri is not null
            && request.RequestUri.IsAbsoluteUri)
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(request.RequestUri.Query), request.RequestUri.Query));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestHeaders) && request.Content is not null)
        {
            if (request.Content.Headers.Any())
            {
                parameters ??= new List<KeyValuePair<string, object?>>();
            }

            foreach (var (key, value) in request.Content.Headers)
            {
                if (!_options.RequestHeaders.Contains(key))
                {
                    parameters!.Add(new(key, Redacted));

                    continue;
                }

                parameters!.Add(new(key, string.Join(',', value)));
            }
        }

        if (parameters?.Count > 0)
        {
            var httpRequestLog = new HttpLog(parameters, "Request");
            _logger.LogInformationRequestLog(httpRequestLog);
        }
    }

    /// <summary>
    /// Logs the HTTP request body if the logging options allow it.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>  
    private async Task LogRequestBodyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(HttpClientLoggingFields.RequestBody))
        {
            return;
        }

        if (request.Content?.Headers.ContentType is not { MediaType: not null, CharSet: not null } requestContentTypeHeader)
        {
            _logger.LogDebugRequestNoMediaType();

            return;
        }

        var mediaType = requestContentTypeHeader.MediaType;
        var charSet = requestContentTypeHeader.CharSet;

        var matchedType = _options.TextContentTypes.FirstOrDefault(x =>
            x.MatchesMediaType(mediaType) &&
            string.Equals(x.Charset.Value, charSet, StringComparison.OrdinalIgnoreCase));

        if (matchedType is null || matchedType.Encoding is null)
        {
            _logger.LogDebugUnrecognizedRequestMediaType();

            return;
        }

        await request.Content.LoadIntoBufferAsync(cancellationToken);
        using var stream = await request.Content.ReadAsStreamAsync(cancellationToken);

        if (stream.Length == 0)
        {
            return;
        }

        var bufferSize = (int)Math.Min(stream.Length, _options.ResponseBodyLogLimit);
        var buffer = new byte[bufferSize];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

        string bodyString;

        try
        {
            bodyString = matchedType.Encoding.GetString(buffer, 0, bytesRead);
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogDebugDecodeFailure(ex);
            bodyString = "<Decoder failure>";
        }

        _logger.LogInformationRequestBody(bodyString);
    }

    /// <summary>
    /// Logs the HTTP response properties and headers based on the configured logging fields.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response"/> is null.</exception>  
    private void LogResponsePropertiesAndHeaders(HttpResponseMessage response)
    {
        List<KeyValuePair<string, object?>>? parameters = null;

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.ResponseStatusCode))
        {
            parameters ??= new List<KeyValuePair<string, object?>>();
            parameters.Add(new(nameof(response.StatusCode), response.StatusCode));
        }

        if (_options.LoggingFields.HasFlag(HttpClientLoggingFields.ResponseHeaders))
        {
            if (response.Content.Headers.Any())
            {
                parameters ??= new List<KeyValuePair<string, object?>>();
            }

            foreach (var (key, value) in response.Content.Headers)
            {
                if (!_options.ResponseHeaders.Contains(key))
                {
                    parameters!.Add(new(key, Redacted));

                    continue;
                }

                parameters!.Add(new(key, string.Join(',', value)));
            }
        }

        if (parameters?.Count > 0)
        {
            var httpResponseLog = new HttpLog(parameters, "Response");
            _logger.LogInformationResponseLog(httpResponseLog);
        }
    }

    /// <summary>
    /// Logs the HTTP response body if the logging options allow it.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response"/> is null.</exception>  
    private async Task LogResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(HttpClientLoggingFields.ResponseBody))
        {
            return;
        }

        if (response.Content.Headers.ContentType is not { MediaType: not null, CharSet: not null } responseContentTypeHeader)
        {
            _logger.LogDebugResponseNoMediaType();

            return;
        }

        var mediaType = responseContentTypeHeader.MediaType;
        var charSet = responseContentTypeHeader.CharSet;

        var matchedType = _options.TextContentTypes.FirstOrDefault(x =>
            x.MatchesMediaType(mediaType) &&
            string.Equals(x.Charset.Value, charSet, StringComparison.OrdinalIgnoreCase));

        if (matchedType is null || matchedType.Encoding is null)
        {
            _logger.LogDebugUnrecognizedResponseMediaType();

            return;
        }

        await response.Content.LoadIntoBufferAsync(cancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (stream.Length == 0)
        {
            return;
        }

        var bufferSize = (int)Math.Min(stream.Length, _options.ResponseBodyLogLimit);
        var buffer = new byte[bufferSize];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

        string bodyString;

        try
        {
            bodyString = matchedType.Encoding.GetString(buffer, 0, bytesRead);
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogDebugDecodeFailure(ex);
            bodyString = "<Decoder failure>";
        }

        _logger.LogInformationResponseBody(bodyString);
    }

    /// <summary>
    /// Logs the duration of the HTTP request-response round trip if enabled.
    /// </summary>
    /// <param name="startTimestamp">The start timestamp of the request as provided by <see cref="TimeProvider"/>.</param>
    private void LogDuration(long startTimestamp)
    {
        if (!_options.LoggingFields.HasFlag(HttpClientLoggingFields.Duration))
        {
            return;
        }

        _logger.LogInformationDuration(_timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds);
    }
}

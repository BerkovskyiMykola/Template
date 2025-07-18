using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs HTTP request and response details
/// according to configured logging options.
/// </summary>
/// <param name="options">The options controlling which parts of the request/response are logged.</param>
/// <param name="timeProvider">The time provider used to measure request duration.</param>
/// <param name="logger">The logger used for logging messages.</param>
internal sealed class LoggerHandler(
    LoggerHandlerOptions options,
    TimeProvider timeProvider,
    ILogger logger) : DelegatingHandler
{
    private readonly static Encoding _defaultEncoding = Encoding.UTF8;

    private readonly LoggerHandlerOptions _options = options;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends an HTTP request asynchronously and logs the request and response based on the configured options.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogLevel.Information) || _options.LoggingFields == LoggingFields.None)
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
    private void LogRequestPropertiesAndHeaders(HttpRequestMessage request)
    {
        var parameters = new List<KeyValuePair<string, object?>>();

        if (_options.LoggingFields.HasFlag(LoggingFields.RequestProtocol))
        {
            parameters.Add(new("Protocol", $"HTTP/{request.Version}"));
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.RequestMethod))
        {
            parameters.Add(new(nameof(request.Method), request.Method));
        }

        if (request.RequestUri is { IsAbsoluteUri: true } uri)
        {
            if (_options.LoggingFields.HasFlag(LoggingFields.RequestScheme))
            {
                parameters.Add(new(nameof(uri.Scheme), uri.Scheme));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.RequestHost))
            {
                parameters.Add(new("Host", $"{uri.Host}:{uri.Port}"));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.RequestAbsolutePath))
            {
                parameters.Add(new(nameof(uri.AbsolutePath), uri.AbsolutePath));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.RequestQuery))
            {
                parameters.Add(new(nameof(uri.Query), uri.Query));
            }
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.RequestHeaders) 
            && request.Content is not null)
        {
            AddAllowedOrRedactedHeaders(request.Content.Headers, parameters, _options.RequestHeaders);
        }

        if (parameters.Count > 0)
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
    private async Task LogRequestBodyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(LoggingFields.RequestBody))
        {
            return;
        }

        if (request.Content?.Headers.ContentType is not { MediaType: not null } requestContentTypeHeader)
        {
            _logger.LogDebugRequestNoMediaType();

            return;
        }

        var matchedType = MatchTextContentType(requestContentTypeHeader.MediaType, requestContentTypeHeader.CharSet);

        if (matchedType is null)
        {
            _logger.LogDebugUnrecognizedRequestMediaType();

            return;
        }

        var bodyString = await ReadContentAsStringOrDefaultAsync(request.Content, matchedType.Encoding, _options.RequestBodyLogLimit, cancellationToken);

        if (bodyString is null)
        {
            return;
        }

        _logger.LogInformationRequestBody(bodyString);
    }

    /// <summary>
    /// Logs the HTTP response properties and headers based on the configured logging fields.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    private void LogResponsePropertiesAndHeaders(HttpResponseMessage response)
    {
        var parameters = new List<KeyValuePair<string, object?>>();

        if (_options.LoggingFields.HasFlag(LoggingFields.ResponseStatusCode))
        {
            parameters.Add(new(nameof(response.StatusCode), (int)response.StatusCode));
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.ResponseHeaders))
        {
            AddAllowedOrRedactedHeaders(response.Content.Headers, parameters, _options.ResponseHeaders);
        }

        if (parameters.Count > 0)
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
    private async Task LogResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(LoggingFields.ResponseBody))
        {
            return;
        }

        if (response.Content.Headers.ContentType is not { MediaType: not null } responseContentTypeHeader)
        {
            _logger.LogDebugResponseNoMediaType();

            return;
        }

        var matchedType = MatchTextContentType(responseContentTypeHeader.MediaType, responseContentTypeHeader.CharSet);

        if (matchedType is null)
        {
            _logger.LogDebugUnrecognizedResponseMediaType();

            return;
        }

        var bodyString = await ReadContentAsStringOrDefaultAsync(response.Content, matchedType.Encoding, _options.ResponseBodyLogLimit, cancellationToken);

        if (bodyString is null)
        {
            return;
        }

        _logger.LogInformationResponseBody(bodyString);
    }

    /// <summary>
    /// Logs the duration of the HTTP request-response round trip if enabled.
    /// </summary>
    /// <param name="startTimestamp">The start timestamp of the request as provided by <see cref="TimeProvider"/>.</param>
    private void LogDuration(long startTimestamp)
    {
        if (!_options.LoggingFields.HasFlag(LoggingFields.Duration))
        {
            return;
        }

        _logger.LogInformationDuration(_timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds);
    }

    /// <summary>
    /// Matches the provided media type and charset against known text content types.
    /// </summary>
    /// <param name="mediaType">The media type to match (e.g., "text/plain").</param>
    /// <param name="charset">The optional charset (e.g., "utf-8"). If null, a <see cref="_defaultEncoding"/> is used.</param>
    /// <returns>
    /// A <see cref="TextContentTypeMatch"/> instance if a matching content type is found; otherwise, <c>null</c>.
    /// </returns>
    private TextContentTypeMatch? MatchTextContentType(string mediaType, string? charset)
    {
        var matchedType = _options.TextContentTypes.FirstOrDefault(x =>
            x.MatchesMediaType(mediaType) &&
            string.Equals(x.Charset.Value ?? _defaultEncoding.BodyName, charset ?? _defaultEncoding.BodyName, StringComparison.OrdinalIgnoreCase));

        if (matchedType is null)
        {
            return null;
        }

        return new TextContentTypeMatch(
            matchedType.Encoding ?? _defaultEncoding);
    }

    private record TextContentTypeMatch(Encoding Encoding);

    /// <summary>
    /// Reads the HTTP content as a string using the provided encoding and respecting the log limit.
    /// </summary>
    /// <param name="content">The HTTP content to read.</param>
    /// <param name="encoding">The encoding to use when converting bytes to a string.</param>
    /// <param name="logLimit">The maximum number of bytes to read from the stream.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="string"/> representation of the content if readable; otherwise, <c>null</c>.
    /// Returns <c>"&lt;Decoder failure&gt;"</c> if decoding fails due to a <see cref="DecoderFallbackException"/>.
    /// </returns>
    private async Task<string?> ReadContentAsStringOrDefaultAsync(HttpContent content, Encoding encoding, long logLimit, CancellationToken cancellationToken)
    {
        await content.LoadIntoBufferAsync(cancellationToken);

        using var stream = await content.ReadAsStreamAsync(cancellationToken);

        if (stream.Length == 0)
        {
            return null;
        }
        
        var bufferSize = (int)Math.Min(stream.Length, logLimit);
        var buffer = new byte[bufferSize];

        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        try
        {
            return encoding.GetString(buffer, 0, bytesRead);
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogDebugDecodeFailure(ex);
            return "<Decoder failure>";
        }
    }

    /// <summary>
    /// Adds HTTP headers to the parameter list, redacting those not included in the allowed headers set.
    /// </summary>
    /// <param name="headers">The HTTP content headers to process.</param>
    /// <param name="parameters">The list of parameters to which header key-value pairs will be added.</param>
    /// <param name="allowedHeaders">A set of allowed header names. Headers not in this set will be redacted.</param>
    private void AddAllowedOrRedactedHeaders(HttpContentHeaders headers, List<KeyValuePair<string, object?>> parameters, ISet<string> allowedHeaders)
    {
        const string Redacted = "[Redacted]";

        foreach (var (key, value) in headers)
        {
            if (!allowedHeaders.Contains(key))
            {
                parameters!.Add(new(key, Redacted));
            }
            else
            {
                parameters!.Add(new(key, string.Join(',', value)));
            }
        }
    }
}

using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs <see cref="HttpResponseMessage"/>
/// according to configured <see cref="HandlerOptions"/>.
/// </summary>
/// <param name="options">The options controlling which parts of the <see cref="HttpResponseMessage"/> are logged.</param>
/// <param name="logger">The logger used for logging <see cref="HttpResponseMessage"/> according to configured <paramref name="options"/>.</param>
internal sealed class Handler(
    HandlerOptions options,
    ILogger logger) : DelegatingHandler
{
    private readonly HandlerOptions _options = options;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends an <paramref name="request"/> asynchronously and logs the <see cref="HttpResponseMessage"/> based on the configured <see cref="HandlerOptions"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogLevel.Information) || _options.LoggingFields is LoggingFields.None)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        LogResponsePropertiesAndHeaders(response);

        await LogResponseBodyAsync(response, cancellationToken).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    /// Logs the <see cref="HttpResponseMessage"/> properties and headers based on the configured <see cref="HandlerOptions"/>.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> whose properties and headers will be logged if allowed by the <see cref="HandlerOptions"/>.</param>
    private void LogResponsePropertiesAndHeaders(HttpResponseMessage response)
    {
        var log = new List<KeyValuePair<string, object?>>();

        if (_options.LoggingFields.HasFlag(LoggingFields.StatusCode))
        {
            log.Add(new(nameof(response.StatusCode), (int)response.StatusCode));
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.Headers))
        {
            Helper.AddAllowedOrRedactedHeadersToLog(log, response.Headers, _options.AllowedHeaders);
        }

        if (log.Count > 0)
        {
            _logger.LogResponseLogAsInformation(log);
        }
    }

    /// <summary>
    /// Logs the <see cref="HttpResponseMessage.Content"/> if the <see cref="HandlerOptions"/> allow it.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> whose <see cref="HttpResponseMessage.Content"/> will be logged if allowed by the <see cref="HandlerOptions"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    private async Task LogResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(LoggingFields.Body))
        {
            return;
        }

        if (response.Content.Headers.ContentType is not { MediaType: not null } responseContentTypeHeader)
        {
            _logger.LogResponseNoMediaTypeAsDebug();

            return;
        }

        if (!Helper.TryGetEncodingForMediaType(
            responseContentTypeHeader.ToString(),
            _options.AllowedMediaTypes.MediaTypeStates,
            out Encoding? encoding))
        {
            _logger.LogUnrecognizedResponseMediaTypeAsDebug();

            return;
        }

        var bodyString = await Helper.ReadContentAsStringOrDefaultAsync(response.Content, encoding, _options.BodyLogLimit, _logger, cancellationToken).ConfigureAwait(false);

        if (bodyString is null)
        {
            return;
        }

        _logger.LogResponseBodyAsInformation(bodyString);
    }
}

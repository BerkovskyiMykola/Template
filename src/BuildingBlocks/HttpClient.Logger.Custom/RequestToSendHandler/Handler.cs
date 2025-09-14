/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.RequestToSendHandler;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs <see cref="HttpRequestMessage"/>
/// according to configured <see cref="HandlerOptions"/>.
/// </summary>
/// <param name="options">The options controlling which parts of the <see cref="HttpRequestMessage"/> are logged.</param>
/// <param name="logger">The logger used for logging <see cref="HttpRequestMessage"/> according to configured <paramref name="options"/>.</param>
internal sealed class Handler(
    HandlerOptions options,
    ILogger logger) : DelegatingHandler
{
    private readonly HandlerOptions _options = options;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends an <paramref name="request"/> asynchronously and logs the <paramref name="request"/> based on the configured <see cref="HandlerOptions"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send and log its properties based on the configured <see cref="HandlerOptions"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogLevel.Information) || _options.LoggingFields is LoggingFields.None)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        LogRequestPropertiesAndHeadersToSend(request);

        await LogRequestBodyToSendAsync(request, cancellationToken).ConfigureAwait(false);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void LogRequestPropertiesAndHeadersToSend(HttpRequestMessage request)
    {
        List<LogField> log = [];

        if (_options.LoggingFields.HasFlag(LoggingFields.Protocol))
        {
            log.Add(new("Protocol", $"HTTP/{request.Version}"));
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.Method))
        {
            log.Add(new(nameof(request.Method), request.Method));
        }

        if (request.RequestUri is { IsAbsoluteUri: true } uri)
        {
            if (_options.LoggingFields.HasFlag(LoggingFields.Scheme))
            {
                log.Add(new(nameof(uri.Scheme), uri.Scheme));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.Host))
            {
                log.Add(new("Host", $"{uri.Host}:{uri.Port}"));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.AbsolutePath))
            {
                log.Add(new(nameof(uri.AbsolutePath), uri.AbsolutePath));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.Query))
            {
                log.Add(new(nameof(uri.Query), uri.Query));
            }
        }

        if (_options.LoggingFields.HasFlag(LoggingFields.Headers))
        {
            Helper.AddAllowedOrRedactedHeadersToLog(log, request.Headers, _options.AllowedHeaders);
        }

        if (log.Count > 0)
        {
            _logger.LogRequestToSendLogAsInformation(log);
        }
    }

    private async Task LogRequestBodyToSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.LoggingFields.HasFlag(LoggingFields.Body))
        {
            return;
        }

        if (request.Content?.Headers.ContentType is not { MediaType: not null } сontentTypeHeader)
        {
            _logger.LogRequestToSendNoMediaTypeAsDebug();

            return;
        }

        if (!Helper.TryGetEncodingForMediaType(
            сontentTypeHeader.ToString(),
            _options.AllowedMediaTypes.MediaTypeStates,
            out Encoding? encoding))
        {
            _logger.LogUnrecognizedRequestToSendMediaTypeAsDebug();

            return;
        }

        string? body = await Helper.ReadContentAsStringOrDefaultAsync(request.Content, encoding, _options.BodyLogLimit, _logger, cancellationToken).ConfigureAwait(false);

        if (body is null)
        {
            return;
        }

        _logger.LogRequestBodyToSendAsInformation(body);
    }
}

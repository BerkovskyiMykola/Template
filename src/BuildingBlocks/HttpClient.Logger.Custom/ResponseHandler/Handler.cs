/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using LogField = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs <see cref="HttpResponseMessage"/>
/// according to configured <see cref="HandlerOptions"/>.
/// </summary>
/// <param name="options">The options controlling which parts of the <see cref="HttpResponseMessage"/> are logged.</param>
/// <param name="objectPoolPooledLogFieldList">The object pool for <see cref="PooledStringNullableObjectPairList"/> instances.</param>
/// <param name="logger">The logger used for logging <see cref="HttpResponseMessage"/> according to configured <paramref name="options"/>.</param>
internal sealed class Handler(
    HandlerOptions options,
    ObjectPool<PooledStringNullableObjectPairList> objectPoolPooledLogFieldList,
    ILogger logger) : DelegatingHandler
{
    private readonly HandlerOptions _options = options;
    private readonly ObjectPool<PooledStringNullableObjectPairList> _objectPoolPooledLogFieldList = objectPoolPooledLogFieldList;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends an <paramref name="request"/> asynchronously and logs the <see cref="HttpResponseMessage"/> based on the configured <see cref="HandlerOptions"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        bool shouldLog = _logger.IsEnabled(LogLevel.Information) && _options.LoggingFields is not LoggingFields.None;

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (shouldLog)
        {
            LogResponsePropertiesAndHeaders(response);

            await LogResponseBodyAsync(response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private void LogResponsePropertiesAndHeaders(HttpResponseMessage response)
    {
        PooledStringNullableObjectPairList pooledLogFieldList = _objectPoolPooledLogFieldList.Get();

        List<LogField> log = pooledLogFieldList.Items;

        try
        {
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
        finally
        {
            _objectPoolPooledLogFieldList.Return(pooledLogFieldList);
        }
    }

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

        string? body = await Helper.ReadContentAsStringOrDefaultAsync(response.Content, encoding, _options.BodyLogLimit, _logger, cancellationToken).ConfigureAwait(false);

        if (body is null)
        {
            return;
        }

        _logger.LogResponseBodyAsInformation(body);
    }
}

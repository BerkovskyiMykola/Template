/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// Logs <see cref="HttpResponseMessage"/> according to configured <see cref="HandlerOptions"/>.
/// </summary>
internal sealed class Handler : DelegatingHandler
{
    private readonly HandlerOptions _options;
    private readonly ObjectPool<ResettableStringNullableObjectPairList> _objectPoolStringNullableObjectPairList;
    private readonly ILogger _logger;

    ///<summary>
    /// Initializes a new instance of the <see cref="Handler"/>.
    /// </summary>
    /// <param name="options">Controls which parts of the <see cref="HttpResponseMessage"/> are logged.</param>
    /// <param name="objectPoolStringNullableObjectPairList">Provides pooled <see cref="ResettableStringNullableObjectPairList"/> instances.</param>
    /// <param name="logger">Used to log <see cref="HttpResponseMessage"/> entries.</param>
    public Handler(
        HandlerOptions options,
        ObjectPool<ResettableStringNullableObjectPairList> objectPoolStringNullableObjectPairList,
        ILogger logger)
    {
        #if DEBUG
        Guard.IsNotNull(options);
        Guard.IsNotNull(objectPoolStringNullableObjectPairList);
        Guard.IsNotNull(logger);
        #endif

        _options = options;
        _objectPoolStringNullableObjectPairList = objectPoolStringNullableObjectPairList;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        #if DEBUG
        Guard.IsNotNull(request);
        #endif

        bool shouldLog = 
            _logger.IsEnabled(LogLevel.Information) && 
            _options.LoggingFields is not LoggingFields.None;

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (shouldLog)
        {
            LogResponsePropertiesAndHeaders(response);

            await LogResponseBodyAsync(response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private void LogResponsePropertiesAndHeaders(HttpResponseMessage response)
    {
        #if DEBUG
        Guard.IsNotNull(response);
        #endif

        ResettableStringNullableObjectPairList pooledLogFieldList = 
            _objectPoolStringNullableObjectPairList.Get();

        List<StringNullableObjectPair> log = pooledLogFieldList;

        try
        {
            if (_options.LoggingFields.HasFlag(LoggingFields.StatusCode))
            {
                log.Add(new(nameof(response.StatusCode), (int)response.StatusCode));
            }

            if (_options.LoggingFields.HasFlag(LoggingFields.Headers))
            {
                Helper.AddAllowedOrRedactedHeadersToCollection(
                    log, 
                    response.Headers, 
                    _options.AllowedHeaders);
            }

            if (log.Count > 0)
            {
                _logger.LogResponseLogAsInformation(log);
            }
        }
        finally
        {
            _objectPoolStringNullableObjectPairList.Return(pooledLogFieldList);
        }
    }

    private async Task LogResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        #if DEBUG
        Guard.IsNotNull(response);
        #endif

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

        await response.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);

        string? body = await 
            Helper.ReadContentAsStringOrDefaultAsync(
                response.Content, 
                encoding, 
                _options.BodyLogLimit, 
                _logger, 
                cancellationToken)
            .ConfigureAwait(false);

        if (body is null)
        {
            return;
        }

        _logger.LogResponseBodyAsInformation(body);
    }
}

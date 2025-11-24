/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom.RequestToSendHandler;

/// <summary>
/// Logs <see cref="HttpRequestMessage"/> according to configured <see cref="HandlerOptions"/>.
/// </summary>
internal sealed class Handler : DelegatingHandler
{
    private readonly HandlerOptions _options;
    private readonly ObjectPool<ResettableStringNullableObjectPairList> _objectPoolStringNullableObjectPairList;
    private readonly ILogger _logger;

    ///<summary>
    /// Initializes a new instance of the <see cref="Handler"/>.
    /// </summary>
    /// <param name="options">Controls which parts of the <see cref="HttpRequestMessage"/> are logged.</param>
    /// <param name="objectPoolStringNullableObjectPairList">Provides pooled <see cref="ResettableStringNullableObjectPairList"/> instances.</param>
    /// <param name="logger">Used to log <see cref="HttpRequestMessage"/> entries.</param>
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

        if (shouldLog)
        {
            LogRequestPropertiesAndHeadersToSend(request);

            await LogRequestBodyToSendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void LogRequestPropertiesAndHeadersToSend(HttpRequestMessage request)
    {
        #if DEBUG
        Guard.IsNotNull(request);
        #endif

        ResettableStringNullableObjectPairList pooledLogFieldList = 
            _objectPoolStringNullableObjectPairList.Get();

        List<StringNullableObjectPair> log = pooledLogFieldList;

        try
        {
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
                Helper.AddAllowedOrRedactedHeadersToCollection(
                    log, 
                    request.Headers, 
                    _options.AllowedHeaders);
            }

            if (log.Count > 0)
            {
                _logger.LogRequestToSendLogAsInformation(log);
            }
        }
        finally
        {
            _objectPoolStringNullableObjectPairList.Return(pooledLogFieldList);
        }
    }

    private async Task LogRequestBodyToSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        #if DEBUG
        Guard.IsNotNull(request);
        #endif

        if (!_options.LoggingFields.HasFlag(LoggingFields.Body))
        {
            return;
        }

        if (request.Content?.Headers.ContentType is not { MediaType: not null } requestContentTypeHeader)
        {
            _logger.LogRequestToSendNoMediaTypeAsDebug();

            return;
        }

        if (!Helper.TryGetEncodingForMediaType(
            requestContentTypeHeader.ToString(),
            _options.AllowedMediaTypes.MediaTypeStates,
            out Encoding? encoding))
        {
            _logger.LogUnrecognizedRequestToSendMediaTypeAsDebug();

            return;
        }

        await request.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);

        string? body = await
            Helper.ReadContentAsStringOrDefaultAsync(
                request.Content,
                encoding,
                _options.BodyLogLimit,
                _logger,
                cancellationToken)
            .ConfigureAwait(false);

        if (body is null)
        {
            return;
        }

        _logger.LogRequestBodyToSendAsInformation(body);
    }
}

/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.DurationHandler;

/// <summary>
/// Logs the duration of <see cref="System.Net.Http.HttpClient"/> operations.
/// </summary>
internal sealed class Handler : DelegatingHandler
{
    private readonly ILogger _logger;

    ///<summary>
    /// Initializes a new instance of the <see cref="Handler"/>.
    /// </summary>
    /// <param name="logger">Used to log operation duration information.</param>
    public Handler(ILogger logger)
    {
        #if DEBUG
        Guard.IsNotNull(logger);
        #endif

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

        Stopwatch stopwatch = Stopwatch.StartNew();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        stopwatch.Stop();

        _logger.LogDurationAsInformation(stopwatch.ElapsedMilliseconds);

        return response;
    }
}

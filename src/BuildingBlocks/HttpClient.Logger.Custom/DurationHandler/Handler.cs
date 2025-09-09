using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HttpClient.Logger.Custom.DurationHandler;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that logs the <see cref="System.Net.Http.HttpClient"/> operation duration.
/// </summary>
/// <param name="logger">The <see cref="ILogger"/> used for logging <see cref="System.Net.Http.HttpClient"/> operation duration information.</param>
internal sealed class Handler(ILogger logger) : DelegatingHandler
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Sends an <paramref name="request"/> asynchronously and logs the <see cref="System.Net.Http.HttpClient"/> operation duration.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        _logger.LogDurationAsInformation(stopwatch.ElapsedMilliseconds);

        return response;
    }
}

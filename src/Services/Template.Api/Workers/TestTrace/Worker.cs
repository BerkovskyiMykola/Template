/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;
using Template.Api.Common.OpenTelemetry;

namespace Template.Api.Workers.TestTrace;

/// <summary>
/// Worker that periodically sends a request to an internal endpoint (<c>/test-trace</c>)
/// to exercise tracing functionality.
/// </summary>
/// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to create <see cref="System.Net.Http.HttpClient"/> instances for sending requests.</param>
/// <param name="configuration">The <see cref="IConfiguration"/> used to read endpoint URLs and other settings.</param>
/// <param name="logger">The <see cref="ILogger"/> used to log worker lifecycle events and errors.</param>
internal sealed class Worker(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly TimeSpan _executionInterval = TimeSpan.FromSeconds(10);
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<Worker> _logger = logger;

    /// <summary>
    /// Executes the worker, periodically sending a request to an internal <c>/test-trace</c> endpoint until <paramref name="stoppingToken"/> is requested.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWorkerRunningAsInformation();

        using PeriodicTimer timer = new(_executionInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                using Activity? activity = Constants.WorkersActivitySource.StartActivity("TestTraceWorker");

                _ = activity?
                    .SetTag("background.service.name", nameof(Worker))
                    .SetTag("background.service.interval", _executionInterval.ToString());

                try
                {
                    await DoWorkAsync().ConfigureAwait(false);
                }
                #pragma warning disable CA1031, S2221
                catch (Exception ex)
                {
                    _ = activity?
                        .SetStatus(ActivityStatusCode.Error)
                        .SetTag("error.type", ex.GetType().FullName);

                    _logger.LogWorkerIterationFailedAsError(ex);
                }
                #pragma warning restore CA1031, S2221
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWorkerStoppingAsInformation();
        }
    }

    private async Task DoWorkAsync()
    {
        System.Net.Http.HttpClient client = _httpClientFactory.CreateClient(Common.HttpClients.ServiceCollectionExtensions.TestTraceNamedHttpClient);
        HttpResponseMessage response = await client
            .PostAsJsonAsync(
                $"{_configuration["ApiBaseAddress"]}/api/v1/test-trace?test=test",
                new
                {
                    Name = "Trace Name"
                })
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }
}

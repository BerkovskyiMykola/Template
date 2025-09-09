using System.Diagnostics;
using Template.Api.Common.OpenTelemetry;

namespace Template.Api.Workers.TestTrace;

/// <summary>
/// Worker that periodically sends a request to an internal endpoint (/test-trace).
/// </summary>
/// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to create <see cref="System.Net.Http.HttpClient"/> to send requests.</param>
/// <param name="configuration">The <see cref="IConfiguration"/> to get endpoint information.</param>
/// <param name="logger">The <see cref="ILogger"/> used for logging worker flow.</param>
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
    /// Executes the worker, periodically invoking the <see cref="DoWork"/> method until <paramref name="stoppingToken"/> is requested.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWorkerRunningAsInformation();

        using var timer = new PeriodicTimer(_executionInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await DoWork().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWorkerStoppingAsInformation();
        }
    }

    /// <summary>
    /// Performs the main work of the worker: sends a request to an internal endpoint (/test-trace).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task DoWork()
    {
        using Activity? activity = Constants.WorkersActivitySource.StartActivity("TestTraceWorker", ActivityKind.Internal);

        _ = activity?
            .SetTag("background.service.name", nameof(Worker))
            .SetTag("background.service.interval", _executionInterval.ToString());
        try
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
        #pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _ = activity?
                .SetStatus(ActivityStatusCode.Error)
                .SetTag("error.type", ex.GetType().FullName);

            _logger.LogWorkerIterationFailedAsError(ex);
        }
        #pragma warning restore CA1031 // Do not catch general exception types
    }
}

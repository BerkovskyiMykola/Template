using System.Diagnostics;

namespace Template.Api.Workers;

/// <summary>
/// Background service that periodically sends a trace request to an external API and logs the result.
/// </summary>
/// <param name="httpClientFactory">The HTTP client factory used to create HTTP clients.</param>
/// <param name="configuration">The application configuration.</param>
/// <param name="logger">The logger instance.</param>
internal sealed class TestTraceWorker(
    IHttpClientFactory httpClientFactory, 
    IConfiguration configuration, 
    ILogger<TestTraceWorker> logger) : BackgroundService
{
    /// <summary>
    /// The interval between each execution of the background task.
    /// </summary>
    private readonly TimeSpan _executionInterval = TimeSpan.FromSeconds(10);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TestTraceWorker> _logger = logger;

    /// <summary>
    /// Executes the background service, periodically invoking the <see cref="DoWork"/> method until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">A token that signals when the service should stop.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(TestTraceWorker)} running");

        using PeriodicTimer timer = new PeriodicTimer(_executionInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{nameof(TestTraceWorker)} is stopping");
        }
    }

    /// <summary>
    /// Performs the main work of the background service: sends a trace request to an external API and logs the result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task DoWork()
    {
        using var activity = Constants.ActivitySource.StartActivity($"{nameof(Workers)}.{nameof(TestTraceWorker)}", ActivityKind.Internal);

        activity?.SetTag("background.service.name", nameof(TestTraceWorker));
        activity?.SetTag("background.service.interval", _executionInterval.ToString());

        try
        {
            var client = _httpClientFactory.CreateClient("TestTrace");
            var response = await client.PostAsJsonAsync($"{_configuration["ApiBaseAddress"]}/test-trace?test=test", new RequestBody
            {
                Name = "Trace Name"
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Trace request succeeded. Response: {Response}", content);
            }
            else
            {
                _logger.LogWarning("Trace request failed. StatusCode: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("error.type", ex.GetType().FullName);

            _logger.LogError(ex, "An unhandled exception occurred while processing the current iteration");
        }
    }
}

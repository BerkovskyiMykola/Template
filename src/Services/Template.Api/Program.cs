using Template.Api.Common.HttpClients;
using Template.Api.Common.HttpLogging;
using Template.Api.Common.OpenTelemetry;
using Template.Api.Common.Serilog;
using Template.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Logging.ClearProviders();

builder.Services
    .AddConfiguredSerilog()
    .AddConfiguredOpenTelemetry(builder.Configuration, builder.Environment);

builder.Services.AddConfiguredHttpLogging(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddConfiguredTestTraceNamedHttpClient(builder.Configuration);

builder.Services.AddHostedService<TestTraceWorker>();

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", async (IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Program> logger) =>
{
    var client = httpClientFactory.CreateClient(Template.Api.Common.HttpClients.ServiceCollectionExtensions.TestTraceNamedHttpClient);
    var response = await client.PostAsJsonAsync($"{configuration["ApiBaseAddress"]}/test-trace?test=test", new RequestBody
    {
        Name = "Trace Name"
    });

    return $"Hello World! Test trace {await response.Content.ReadAsStringAsync()}";
});

app.MapPost("/test-trace", (RequestBody request) =>
{
    return Results.Ok(
        new ResponseBody
        {
            Name = request.Name
        });
});

await app.RunAsync();

public sealed class RequestBody
{
    public string? Name { get; set; }
}


public sealed class ResponseBody
{
    public string? Name { get; set; }
}
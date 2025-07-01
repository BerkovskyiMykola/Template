using System.Text;
using HttpClient.Logger.Custom;
using Microsoft.Net.Http.Headers;
using Template.Api.Common.HttpLogging;
using Template.Api.Common.Logging;
using Template.Api.Common.OpenTelemetry;
using Template.Api.Common.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Logging.ClearProviders();

builder.Services
    .AddConfiguredSerilog()
    .AddConfiguredOpenTelemetry(builder.Configuration, builder.Environment);

builder.Services.AddConfiguredHttpLogging(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

const string httpClientName = "TestTrace";
builder.Services.AddHttpClient(httpClientName)
    .RemoveAllLoggers()
    .AddCustomLogger(config =>
    {
        var loggingFields = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:LoggingFields").Get<HttpClientLoggingFields>();
        var requestHeaders = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:RequestHeaders").Get<string[]>() ?? [];
        var responseHeaders = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:ResponseHeaders").Get<string[]>() ?? [];
        var textContentTypes = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:TextContentTypes").Get<TextContentTypeOptions[]>() ?? [];
        var requestBodyLogLimit = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:RequestBodyLogLimit").Get<int>();
        var responseBodyLogLimit = builder.Configuration.GetSection($"HttpClientLogging:{httpClientName}:ResponseBodyLogLimit").Get<int>();

        config.LoggingFields = loggingFields;

        foreach (var header in requestHeaders) config.RequestHeaders.Add(header);

        foreach (var header in responseHeaders) config.ResponseHeaders.Add(header);

        foreach (var textContentType in textContentTypes)
        {
            config.TextContentTypes.Add(new MediaTypeHeaderValue(textContentType.MediaType)
            {
                Encoding = Encoding.GetEncoding(textContentType.Encoding)
            });
        }

        config.RequestBodyLogLimit = requestBodyLogLimit;
        config.ResponseBodyLogLimit = responseBodyLogLimit;
    });

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", async (IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Program> logger) =>
{
    logger.LogInformationSomething(
        "This is a log message from the root endpoint.");

    logger.LogErrorSomething(
        "This is an error message from the root endpoint.",
        new InvalidOperationException("This is an error message from the root endpoint."));

    var client = httpClientFactory.CreateClient(httpClientName);
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

internal sealed record TextContentTypeOptions
{
    public string MediaType { get; init; } = string.Empty;
    public string Encoding { get; init; } = string.Empty;
}
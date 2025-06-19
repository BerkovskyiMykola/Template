using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Template.Api.Common.Logging;
using Template.Api.Common.Middlewares;

var applicationName = Assembly.GetEntryAssembly()?.GetName().Name 
    ?? throw new ArgumentNullException("applicationName", "Unable to determine the application name from the executing assembly.");
var serviceInstanceId = Ulid.NewUlid().ToString();

var builder = WebApplication.CreateBuilder(args);

var otelExporterOtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

builder.Logging.ClearProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdMiddleware>();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration.ReadFrom.Configuration(services.GetRequiredService<IConfiguration>());
    configuration.ReadFrom.Services(services);

    if (!string.IsNullOrWhiteSpace(otelExporterOtlpEndpoint))
    {
        configuration.WriteTo.OpenTelemetry(
            endpoint: otelExporterOtlpEndpoint,
            resourceAttributes: new Dictionary<string, object>
            {
                ["service.name"] = applicationName,
                ["service.instance.id"] = serviceInstanceId
            });
    }

    configuration.Enrich.WithProperty("ApplicationName", applicationName);
    configuration.Enrich.WithProperty("ServiceInstanceId", serviceInstanceId);
});

if (!string.IsNullOrWhiteSpace(otelExporterOtlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(configuration =>
        {
            configuration
                .AddService(
                    serviceName: applicationName,
                    serviceInstanceId: serviceInstanceId)
                .AddEnvironmentVariableDetector()
                .AddTelemetrySdk();
        })
        .WithMetrics(configuration =>
        {
            configuration
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            configuration.AddOtlpExporter();
        })
        .WithTracing(configuration =>
        {
            if (builder.Environment.IsDevelopment())
            {
                configuration.SetSampler<AlwaysOnSampler>();
            }

            configuration
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation();

            configuration.AddOtlpExporter();
        });
}

var loggingFields = builder.Configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
var requestHeaders = builder.Configuration.GetSection("HttpLogging:RequestHeaders").Get<string[]>() ?? [];
var responseHeaders = builder.Configuration.GetSection("HttpLogging:ResponseHeaders").Get<string[]>() ?? [];
var textMediaTypes = builder.Configuration.GetSection("HttpLogging:TextMediaTypes").Get<string[]>() ?? [];
var requestBodyLogLimit = builder.Configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
var responseBodyLogLimit = builder.Configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

builder.Services.AddHttpLogging(configuration =>
{
    configuration.LoggingFields = loggingFields;

    configuration.RequestHeaders.Clear();

    foreach (var header in requestHeaders)
    {
        configuration.RequestHeaders.Add(header);
    }

    configuration.ResponseHeaders.Clear();

    foreach (var header in responseHeaders)
    {
        configuration.ResponseHeaders.Add(header);
    }

    configuration.MediaTypeOptions.Clear();

    foreach (var mediaType in textMediaTypes)
    {
        configuration.MediaTypeOptions.AddText(mediaType, Encoding.UTF8);
    }

    configuration.RequestBodyLogLimit = requestBodyLogLimit;
    configuration.ResponseBodyLogLimit = responseBodyLogLimit;
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpLogging();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformationSomething(
        "This is a log message from the root endpoint.");

    logger.LogErrorSomething(
        "This is an error message from the root endpoint.", 
        new InvalidOperationException("This is an error message from the root endpoint."));

    return "Hello World!";
});

await app.RunAsync();

using Template.Api.Common.HttpLogging;
using Template.Api.Common.Logging;
using Template.Api.Common.Middlewares;
using Template.Api.Common.OpenTelemetry;
using Template.Api.Common.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Logging.ClearProviders();

builder.Services
    .AddConfiguredSerilog()
    .AddConfiguredOpenTelemetry(builder.Configuration, builder.Environment);

builder.Services.AddConfiguredHttpLogging(builder.Configuration);

builder.Services.AddTransient<CorrelationIdMiddleware>();

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

/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Api.Common.Logging;
using Api.Common.Telemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Observability
builder.Services.AddRedaction();
builder.Logging.EnableRedaction();
builder.Logging.AddFileIfConfigured(builder.Configuration);
builder.Services.AddOpenTelemetryIfConfigured(builder.Configuration, builder.Environment);
builder.Services.AddHttpLoggingIfConfigured(builder.Configuration);

// Serialization / Formatting
builder.Services.ConfigureHttpJsonOptions(options => { });
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

app.UseHttpLoggingIfConfigured(app.Configuration);

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    #pragma warning disable IDE0058
    app.UseDeveloperExceptionPage();
    #pragma warning restore IDE0058
}

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);

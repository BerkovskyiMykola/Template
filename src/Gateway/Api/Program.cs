/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Api.Common.Logging;
using Api.Common.Telemetry;
using Microsoft.Extensions.AmbientMetadata;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

///Ambient metadata
builder.Services.AddBuildMetadata(builder.Configuration.GetSection("AmbientMetadata:Build"));

builder.UseApplicationMetadata("AmbientMetadata:Application");

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

app.MapGet("api/ambient-metadata/build", static (IOptions<BuildMetadata> options) => options.Value);

app.MapGet("api/ambient-metadata/application", static (IOptions<ApplicationMetadata> options) => options.Value);

await app.RunAsync().ConfigureAwait(false);

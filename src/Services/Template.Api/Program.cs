/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Template.Api.Common.Endpoints;
using Template.Api.Common.HttpClients;
using Template.Api.Common.HttpLogging;
using Template.Api.Common.LoggingProviders;
using Template.Api.Common.OpenTelemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration

// Observability
builder.Logging.AddConfiguredProviders(builder.Configuration);
builder.Services.AddConfiguredOpenTelemetry(builder.Configuration, builder.Environment);

// Serialization / Formatting
builder.Services.ConfigureHttpJsonOptions(config => { });
builder.Services.AddProblemDetails();

// Diagnostics
builder.Services.AddConfiguredHttpLogging(builder.Configuration);

// Core services
builder.Services.AddSingleton(TimeProvider.System);

// Application Services
builder.Services.AddConfiguredTestTraceNamedHttpClient();

builder.Services.AddHostedService<Template.Api.Workers.TestTrace.Worker>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    #pragma warning disable IDE0058
    app.UseDeveloperExceptionPage();
    #pragma warning restore IDE0058
}

app.UseHttpLogging();

app.MapEndpoints();

await app.RunAsync().ConfigureAwait(false);

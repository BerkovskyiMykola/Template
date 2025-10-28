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

builder.Logging.AddConfiguredProviders(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddConfiguredOpenTelemetry(builder.Configuration, builder.Environment);

builder.Services.AddConfiguredHttpLogging(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddConfiguredTestTraceNamedHttpClient(builder.Configuration);

builder.Services.AddHostedService<Template.Api.Workers.TestTrace.Worker>();

WebApplication app = builder.Build();

app.UseHttpLogging();

app.MapEndpoints();

await app.RunAsync().ConfigureAwait(false);

/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Template.Api.Common.OpenTelemetry;

/// <summary>  
/// Provides extension methods for configuring and adding OpenTelemetry services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds and configures OpenTelemetry, based on the <paramref name="configuration"/>, to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add configured OpenTelemetry to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing OpenTelemetry service settings.</param>  
    /// <param name="environment">The <see cref="IHostEnvironment"/> to configure OpenTelemetry depending on the environment.</param>
    /// <returns>The <see cref="IServiceCollection"/> with OpenTelemetry configured.</returns>  
    internal static IServiceCollection AddConfiguredOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _ = services.AddOpenTelemetry()
            .ConfigureResource(config =>
            {
                string serviceName = configuration["OpenTelemetry:ResourceAttributes:service.name"]!;
                string serviceInstanceId = configuration["OpenTelemetry:ResourceAttributes:service.instance.id"]!;

                _ = config
                    .AddService(
                        serviceName: serviceName,
                        autoGenerateServiceInstanceId: false,
                        serviceInstanceId: serviceInstanceId)
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            })
            .WithMetrics(config =>
            {
                RegisterInstrumentations(config, configuration, new Dictionary<string, Action<MeterProviderBuilder>>
                {
                    ["OpenTelemetry:Metrics:AddRuntimeInstrumentation"] = static x => x.AddRuntimeInstrumentation(),
                    ["OpenTelemetry:Metrics:AddHttpClientInstrumentation"] = static x => x.AddHttpClientInstrumentation(),
                    ["OpenTelemetry:Metrics:AddAspNetCoreInstrumentation"] = static x => x.AddAspNetCoreInstrumentation()
                });

                RegisterExporters(config, configuration, new Dictionary<string, Action<MeterProviderBuilder, IConfiguration>>
                {
                    ["OpenTelemetry:Exporters:Otlp:Metrics"] = static (x, y) => x.AddOtlpExporter(z => ConfigureOtlpOptions(z, y))
                });
            })
            .WithTracing(config =>
            {
                if (environment.IsDevelopment())
                {
                    _ = config.SetSampler<AlwaysOnSampler>();
                }

                RegisterInstrumentations(config, configuration, new Dictionary<string, Action<TracerProviderBuilder>>
                {
                    ["OpenTelemetry:Tracing:AddHttpClientInstrumentation"] = static x => x.AddHttpClientInstrumentation(),
                    ["OpenTelemetry:Tracing:AddAspNetCoreInstrumentation"] = static x => x.AddAspNetCoreInstrumentation(),
                    ["OpenTelemetry:Tracing:AddEntityFrameworkCoreInstrumentation"] = static x => x.AddEntityFrameworkCoreInstrumentation(),
                    ["OpenTelemetry:Tracing:AddWorkersInstrumentation"] = static x => x.AddSource(Constants.WorkersActivitySource.Name)
                });

                RegisterExporters(config, configuration, new Dictionary<string, Action<TracerProviderBuilder, IConfiguration>>
                {
                    ["OpenTelemetry:Exporters:Otlp:Tracing"] = static (x, y) => x.AddOtlpExporter(z => ConfigureOtlpOptions(z, y))
                });
            });

        return services;
    }

    private static void RegisterInstrumentations<TBuilder>(
        TBuilder builder,
        IConfiguration configuration,
        IReadOnlyDictionary<string, Action<TBuilder>> registrations)
    {
        foreach ((string key, Action<TBuilder> value) in registrations)
        {
            if (configuration.GetValue<bool>(key))
            {
                value(builder);
            }
        }
    }

    private static void RegisterExporters<TBuilder>(
        TBuilder builder,
        IConfiguration configuration,
        IReadOnlyDictionary<string, Action<TBuilder, IConfiguration>> registrations)
    {
        foreach ((string key, Action<TBuilder, IConfiguration> action) in registrations)
        {
            IConfigurationSection section = configuration.GetSection(key);
            if (section.Exists())
            {
                action(builder, section);
            }
        }
    }

    private static void ConfigureOtlpOptions(OtlpExporterOptions options, IConfiguration configuration)
    {
        options.Endpoint = new Uri(configuration["Endpoint"]!);
        options.Protocol = configuration.GetSection("Protocol").Get<OtlpExportProtocol>();

        Dictionary<string, string>? headers = configuration.GetSection("Headers").Get<Dictionary<string, string>>();
        if (headers is { Count: > 0 })
        {
            options.Headers = string.Join(",", headers.Select(static h => $"{h.Key}={h.Value}"));
        }
    }
}

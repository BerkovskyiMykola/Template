/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Template.Api.Common.OpenTelemetry;

/// <summary>  
/// Provides extension methods for configuring and adding OpenTelemetry services to the application. 
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
    public static IServiceCollection AddConfiguredOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        IConfigurationSection openTelemetrySection = configuration.GetSection("OpenTelemetry");

        if (!openTelemetrySection.Exists())
        {
            return services;
        }

        string serviceName = openTelemetrySection.GetValue<string>("ResourceAttributes:service.name") ?? string.Empty;
        string serviceInstanceId = openTelemetrySection.GetValue<string>("ResourceAttributes:service.instance.id") ?? string.Empty;

        OpenTelemetryBuilder builder = services.AddOpenTelemetry()
            .ConfigureResource(config => config
                .AddService(
                    serviceName: serviceName,
                    autoGenerateServiceInstanceId: false,
                    serviceInstanceId: serviceInstanceId)
                .AddEnvironmentVariableDetector()
                .AddTelemetrySdk());

        ConfigureMetrics(builder, openTelemetrySection);
        ConfigureTracing(builder, environment, openTelemetrySection);
        ConfigureLogging(builder, openTelemetrySection);

        return services;
    }

    private static void ConfigureMetrics(OpenTelemetryBuilder builder, IConfiguration section)
    {
        IConfigurationSection metricsSection = section.GetSection("WithMetrics");

        if (!metricsSection.Exists())
        {
            return;
        }

        string[] instrumentations = metricsSection.GetSection("Instrumentations").Get<string[]>() ?? [];
        MyOtlpExporterOptions? otlpExporterOptions = metricsSection.GetSection("Exporters:Otlp").Get<MyOtlpExporterOptions>();

        _ = builder.WithMetrics(config =>
        {
            foreach (string instrumentation in instrumentations)
            {
                switch (instrumentation)
                {
                    case "Runtime":
                        {
                            _ = config.AddRuntimeInstrumentation();
                            break;
                        }
                    case "HttpClient":
                        {
                            _ = config.AddHttpClientInstrumentation();
                            break;
                        }
                    case "AspNetCore":
                        {
                            _ = config.AddAspNetCoreInstrumentation();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            if (otlpExporterOptions is not null)
            {
                _ = config.AddOtlpExporter(options => ConfigureOtlpOptions(options, otlpExporterOptions));
            }
        });
    }

    private static void ConfigureTracing(OpenTelemetryBuilder builder, IHostEnvironment environment, IConfiguration section)
    {
        IConfigurationSection tracingSection = section.GetSection("WithTracing");

        if (!tracingSection.Exists())
        {
            return;
        }

        string[] instrumentations = tracingSection.GetSection("Instrumentations").Get<string[]>() ?? [];
        MyOtlpExporterOptions? otlpExporterOptions = tracingSection.GetSection("Exporters:Otlp").Get<MyOtlpExporterOptions>();

        _ = builder.WithTracing(config =>
        {
            if (environment.IsDevelopment())
            {
                _ = config.SetSampler<AlwaysOnSampler>();
            }

            foreach (string instrumentation in instrumentations)
            {
                switch (instrumentation)
                {
                    case "HttpClient":
                        {
                            _ = config.AddHttpClientInstrumentation();
                            break;
                        }
                    case "AspNetCore":
                        {
                            _ = config.AddAspNetCoreInstrumentation();
                            break;
                        }
                    case "Workers":
                        {
                            _ = config.AddSource(Constants.WorkersActivitySource.Name);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            if (otlpExporterOptions is not null)
            {
                _ = config.AddOtlpExporter(options => ConfigureOtlpOptions(options, otlpExporterOptions));
            }
        });
    }

    private static void ConfigureLogging(OpenTelemetryBuilder builder, IConfiguration section)
    {
        IConfigurationSection loggingSection = section.GetSection("WithLogging");

        if (!loggingSection.Exists())
        {
            return;
        }

        MyOtlpExporterOptions? otlpExporterOptions = loggingSection.GetSection("Exporters:Otlp").Get<MyOtlpExporterOptions>();

        _ = builder.WithLogging(config =>
        {
            if (otlpExporterOptions is not null)
            {
                _ = config.AddOtlpExporter(options => ConfigureOtlpOptions(options, otlpExporterOptions));
            }
        });
    }

    private static void ConfigureOtlpOptions(OtlpExporterOptions options, MyOtlpExporterOptions myOptions)
    {
        options.Endpoint = new Uri(myOptions.Endpoint);
        options.Protocol = myOptions.Protocol;

        if (myOptions.Headers is { Count: > 0 })
        {
            options.Headers = string.Join(",", myOptions.Headers.Select(static h => $"{h.Key}={h.Value}"));
        }
    }

    private sealed record MyOtlpExporterOptions(
        string Endpoint,
        OtlpExportProtocol Protocol,
        IReadOnlyDictionary<string, string> Headers)
    {
        public MyOtlpExporterOptions() : this(string.Empty, default, new Dictionary<string, string>()) { }
    }
}

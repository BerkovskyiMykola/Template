/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Common.Telemetry;

/// <summary>  
/// Extension methods for configuring and adding telemetry services to the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds an OpenTelemetry to the <see cref="IServiceCollection"/> if the
    /// "OpenTelemetry" configuration section exists and has any exporter in the <see cref="IConfiguration"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="services">The services to which the OpenTelemetry will be added.</param>  
    /// <param name="configuration">The configuration used to check whether the OpenTelemetry is configured.</param>  
    /// <param name="hostEnvironment">The host environment used to configure OpenTelemetry depending on the environment.</param>
    /// <returns>The original services.</returns>  
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/>, <paramref name="configuration"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddOpenTelemetryIfConfigured(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        #if DEBUG
        Guard.IsNotNull(services);
        Guard.IsNotNull(configuration);
        Guard.IsNotNull(hostEnvironment);
        #endif

        MyOtelOptions? options = configuration.GetSection("OpenTelemetry").Get<MyOtelOptions>();

        if (options is null || !options.HasAnyExporter)
        {
            return services;
        }

        OpenTelemetryBuilder builder = services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService(
                        options.Resource.ServiceName,
                        serviceInstanceId: options.Resource.ServiceInstanceId)
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            });

        MyMetricsExportersOptions? metricsExportersOptions = options.MetricsExporters;

        if (metricsExportersOptions is not null && metricsExportersOptions.HasAnyExporter)
        {
            _ = builder.WithMetrics(meterProviderBuilder =>
            {
                _ = meterProviderBuilder
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                metricsExportersOptions.ConfigureMeterProviderBuilder(meterProviderBuilder);
            });
        }

        MyTracesExportersOptions? tracesExportersOptions = options.TracesExporters;

        if (tracesExportersOptions is not null && tracesExportersOptions.HasAnyExporter)
        {
            _ = builder.WithTracing(tracerProviderBuilder =>
            {
                _ = tracerProviderBuilder
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                if (hostEnvironment.IsDevelopment())
                {
                    _ = tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                }

                tracesExportersOptions.ConfigureTracerProviderBuilder(tracerProviderBuilder);
            });
        }

        MyLogsExportersOptions? logsExportersOptions = options.LogsExporters;

        if (logsExportersOptions is not null && logsExportersOptions.HasAnyExporter)
        {
            _ = builder.WithLogging(logsExportersOptions.ConfigureLoggerProviderBuilder);
        }

        return services;
    }

    private sealed record MyResourceOptions(
        string ServiceName, 
        string ServiceInstanceId)
    {
        public MyResourceOptions() 
            : this("Gateway.Api", Ulid.NewUlid().ToString())
        {
        }
    }

    private sealed record MyOtlpExporterOptions(
        string Endpoint, 
        OtlpExportProtocol Protocol, 
        IReadOnlyDictionary<string, string> Headers)
    {
        public MyOtlpExporterOptions()
            : this(
                "http://localhost:4317", 
                OtlpExportProtocol.Grpc, 
                new Dictionary<string, string>())
        {
        }

        public void ConfigureOtlpExporterOptions(OtlpExporterOptions options)
        {
            options.Endpoint = new Uri(Endpoint);
            options.Protocol = Protocol;

            if (Headers.Count > 0)
            {
                options.Headers = string.Join(
                    ",", 
                    Headers.Select(static h => $"{h.Key}={h.Value}"));
            }
        }
    }

    private sealed record MyMetricsExportersOptions(
        MyOtlpExporterOptions? Otlp)
    {
        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureMeterProviderBuilder(MeterProviderBuilder builder)
        {
            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyTracesExportersOptions(
        MyOtlpExporterOptions? Otlp)
    {
        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureTracerProviderBuilder(TracerProviderBuilder builder)
        {
            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyLogsExportersOptions(
        MyOtlpExporterOptions? Otlp)
    {
        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureLoggerProviderBuilder(LoggerProviderBuilder builder)
        {
            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyOtelOptions(
        MyResourceOptions Resource,
        MyMetricsExportersOptions? MetricsExporters,
        MyTracesExportersOptions? TracesExporters,
        MyLogsExportersOptions? LogsExporters)
    {
        public MyOtelOptions()
            : this(new(), null, null, null)
        {
        }

        public bool HasAnyExporter 
            => (MetricsExporters?.HasAnyExporter ?? false) 
            || (TracesExporters?.HasAnyExporter ?? false) 
            || (LogsExporters?.HasAnyExporter ?? false);
    }
}

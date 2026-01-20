/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Diagnostics.Extensions;
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
                        hostEnvironment.ApplicationName,
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

    private sealed record MyResourceOptions
    {
        public string ServiceInstanceId
        {
            get;
            init
            {
                Guard.IsNotWhiteSpace(value);

                field = value;
            }
        } = Ulid.NewUlid().ToString();
    }

    private sealed record MyOtlpExporterOptions
    {
        public string Endpoint
        {
            get;
            init
            {
                Guard.IsNotWhiteSpace(value);
                GuardExt.IsUrl(value);

                field = value;
            }
        } = "http://localhost:4317";

        public OtlpExportProtocol Protocol
        {
            get;
            init
            {
                GuardExt.IsDefinedEnum(value);

                field = value;
            }
        } = OtlpExportProtocol.Grpc;

        public Dictionary<string, string> Headers
        {
            get;
            init
            {
                Guard.IsNotNull(value);

                foreach (KeyValuePair<string, string> header in value)
                {
                    if (string.IsNullOrWhiteSpace(header.Key))
                    {
                        throw new ArgumentException("Header key cannot be null or whitespace.", nameof(value));
                    }

                    if (string.IsNullOrWhiteSpace(header.Value))
                    {
                        throw new ArgumentException($"Header '{header.Key}' has null or whitespace value.", nameof(value));
                    }
                }

                field = value;
            }
        } = [];

        public void ConfigureOtlpExporterOptions(OtlpExporterOptions options)
        {
            #if DEBUG
            Guard.IsNotNull(options);
            #endif

            options.Endpoint = new Uri(Endpoint);
            options.Protocol = Protocol;

            if (Headers.Count > 0)
            {
                options.Headers = string.Join(
                    ",", 
                    Headers.Select(static header => $"{header.Key}={header.Value}"));
            }
        }
    }

    private sealed record MyMetricsExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; init; }
        #pragma warning restore S3459, S1144

        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureMeterProviderBuilder(MeterProviderBuilder builder)
        {
            #if DEBUG
            Guard.IsNotNull(builder);
            #endif

            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyTracesExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; init; }
        #pragma warning restore S3459, S1144

        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureTracerProviderBuilder(TracerProviderBuilder builder)
        {
            #if DEBUG
            Guard.IsNotNull(builder);
            #endif

            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyLogsExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; init; }
        #pragma warning restore S3459, S1144

        public bool HasAnyExporter => Otlp is not null;

        public void ConfigureLoggerProviderBuilder(LoggerProviderBuilder builder)
        {
            #if DEBUG
            Guard.IsNotNull(builder);
            #endif

            if (Otlp is not null)
            {
                _ = builder.AddOtlpExporter(Otlp.ConfigureOtlpExporterOptions);
            }
        }
    }

    private sealed record MyOtelOptions
    {
        public MyResourceOptions Resource { get; init; } = new();

        #pragma warning disable S3459, S1144
        public MyMetricsExportersOptions? MetricsExporters { get; init; }
        #pragma warning restore S3459, S1144

        #pragma warning disable S3459, S1144
        public MyTracesExportersOptions? TracesExporters { get; init; }
        #pragma warning restore S3459, S1144

        #pragma warning disable S3459, S1144
        public MyLogsExportersOptions? LogsExporters { get; init; }
        #pragma warning restore S3459, S1144

        public bool HasAnyExporter 
            => (MetricsExporters?.HasAnyExporter ?? false) 
            || (TracesExporters?.HasAnyExporter ?? false) 
            || (LogsExporters?.HasAnyExporter ?? false);
    }
}

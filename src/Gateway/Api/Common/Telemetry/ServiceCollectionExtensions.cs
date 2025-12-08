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

    private sealed class MyResourceOptions
    {
        #pragma warning disable S2325
        public string ServiceName
        #pragma warning restore S2325
        {
            get;
            set
            {
                #if DEBUG
                Guard.IsNotWhiteSpace(value);
                #endif

                field = value;
            }
        } = "Gateway.Api";

        #pragma warning disable S2325
        public string ServiceInstanceId
        #pragma warning restore S2325
        {
            get;
            set
            {
                #if DEBUG
                Guard.IsNotWhiteSpace(value);
                #endif

                field = value;
            }
        } = Ulid.NewUlid().ToString();
    }

    private sealed class MyOtlpExporterOptions
    {
        #pragma warning disable S2325
        public string Endpoint
        #pragma warning restore S2325
        {
            get;
            set
            {
                #if DEBUG
                Guard.IsNotWhiteSpace(value);
                GuardExt.IsUrl(value);
                #endif

                field = value;
            }
        } = "http://localhost:4317";

        #pragma warning disable S2325
        public OtlpExportProtocol Protocol
        #pragma warning restore S2325
        {
            get;
            set
            {
                #if DEBUG
                GuardExt.IsDefinedEnum(value);
                #endif

                field = value;
            }
        } = OtlpExportProtocol.Grpc;

        #pragma warning disable S2325
        public Dictionary<string, string> Headers
        #pragma warning restore S2325
        {
            get;
            set
            {
                #if DEBUG
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
                #endif

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

    private sealed class MyMetricsExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; set; }
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

    private sealed class MyTracesExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; set; }
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

    private sealed class MyLogsExportersOptions
    {
        #pragma warning disable S3459, S1144
        public MyOtlpExporterOptions? Otlp { get; set; }
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

    private sealed class MyOtelOptions
    {
        public MyResourceOptions Resource { get; set; } = new();

        #pragma warning disable S3459, S1144
        public MyMetricsExportersOptions? MetricsExporters { get; set; }
        #pragma warning restore S3459, S1144

        #pragma warning disable S3459, S1144
        public MyTracesExportersOptions? TracesExporters { get; set; }
        #pragma warning restore S3459, S1144

        #pragma warning disable S3459, S1144
        public MyLogsExportersOptions? LogsExporters { get; set; }
        #pragma warning restore S3459, S1144

        public bool HasAnyExporter 
            => (MetricsExporters?.HasAnyExporter ?? false) 
            || (TracesExporters?.HasAnyExporter ?? false) 
            || (LogsExporters?.HasAnyExporter ?? false);
    }
}

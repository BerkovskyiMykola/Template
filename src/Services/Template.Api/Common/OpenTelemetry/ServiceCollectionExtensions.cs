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
    /// Adds and configures OpenTelemetry for the application.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add OpenTelemetry to.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with OpenTelemetry configured.</returns>  
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/>, <paramref name="configuration"/> or <paramref name="environment"/> is null.
    /// </exception> 
    public static IServiceCollection AddConfiguredOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(config =>
            {
                var serviceName = configuration["OpenTelemetry:ResourceAttributes:service.name"]!;
                var serviceInstanceId = configuration["OpenTelemetry:ResourceAttributes:service.instance.id"];

                config
                    .AddService(
                        serviceName: serviceName,
                        autoGenerateServiceInstanceId: false,
                        serviceInstanceId: serviceInstanceId)
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            })
            .WithMetrics(config =>
            {
                var metricsAddRuntimeInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddRuntimeInstrumentation").Get<bool>();
                var metricsAddHttpClientInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddHttpClientInstrumentation").Get<bool>();
                var metricsAddAspNetCoreInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddAspNetCoreInstrumentation").Get<bool>();

                if (metricsAddRuntimeInstrumentation)
                {
                    config.AddRuntimeInstrumentation();
                }

                if (metricsAddHttpClientInstrumentation)
                {
                    config.AddHttpClientInstrumentation();
                }

                if (metricsAddAspNetCoreInstrumentation)
                {
                    config.AddAspNetCoreInstrumentation();
                }

                var otlpExporter = configuration.GetSection($"OpenTelemetry:Exporters:Otlp:Metrics");

                if (otlpExporter.Exists())
                {
                    config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();
                        x.Headers = string.Join(
                            ",",
                            (otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()).Select(x => $"{x.Key}={x.Value}")
                        );
                    });
                }
            })
            .WithTracing(config =>
            {
                var tracingAddHttpClientInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddHttpClientInstrumentation").Get<bool>();
                var tracingAddAspNetCoreInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddAspNetCoreInstrumentation").Get<bool>();
                var tracingAddEntityFrameworkCoreInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddEntityFrameworkCoreInstrumentation").Get<bool>();

                if (environment.IsDevelopment())
                {
                    config.SetSampler<AlwaysOnSampler>();
                }

                if (tracingAddHttpClientInstrumentation)
                {
                    config.AddHttpClientInstrumentation();
                }

                if (tracingAddAspNetCoreInstrumentation)
                {
                    config.AddAspNetCoreInstrumentation();
                }

                if (tracingAddEntityFrameworkCoreInstrumentation)
                {
                    config.AddEntityFrameworkCoreInstrumentation();
                }

                var otlpExporter = configuration.GetSection($"OpenTelemetry:Exporters:Otlp:Tracing");

                if (otlpExporter.Exists())
                {
                    config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();
                        x.Headers = string.Join(
                            ",",
                            (otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()).Select(x => $"{x.Key}={x.Value}")
                        );
                    });
                }
            });

        return services;
    }
}

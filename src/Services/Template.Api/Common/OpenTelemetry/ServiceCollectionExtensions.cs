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
    /// Adds and configures OpenTelemetry to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add configured OpenTelemetry to.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with OpenTelemetry configured.</returns>  
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
                var addRuntimeInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddRuntimeInstrumentation").Get<bool>();
                var addHttpClientInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddHttpClientInstrumentation").Get<bool>();
                var addAspNetCoreInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddAspNetCoreInstrumentation").Get<bool>();

                if (addRuntimeInstrumentation)
                {
                    config.AddRuntimeInstrumentation();
                }

                if (addHttpClientInstrumentation)
                {
                    config.AddHttpClientInstrumentation();
                }

                if (addAspNetCoreInstrumentation)
                {
                    config.AddAspNetCoreInstrumentation();
                }

                var otlpExporter = configuration.GetSection("OpenTelemetry:Exporters:Otlp:Metrics");

                if (otlpExporter.Exists())
                {
                    config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();

                        var headers = otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>();
                        if (headers is { Count: > 0 })
                        {
                            x.Headers = string.Join(",", headers.Select(y => $"{y.Key}={y.Value}"));
                        }
                    });
                }
            })
            .WithTracing(config =>
            {
                var addHttpClientInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddHttpClientInstrumentation").Get<bool>();
                var addAspNetCoreInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddAspNetCoreInstrumentation").Get<bool>();
                var addEntityFrameworkCoreInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddEntityFrameworkCoreInstrumentation").Get<bool>();
                var addWorkersInstrumentation = configuration.GetSection("OpenTelemetry:Tracing:AddWorkersInstrumentation").Get<bool>();

                if (environment.IsDevelopment())
                {
                    config.SetSampler<AlwaysOnSampler>();
                }

                if (addHttpClientInstrumentation)
                {
                    config.AddHttpClientInstrumentation();
                }

                if (addAspNetCoreInstrumentation)
                {
                    config.AddAspNetCoreInstrumentation();
                }

                if (addEntityFrameworkCoreInstrumentation)
                {
                    config.AddEntityFrameworkCoreInstrumentation();
                }

                if (addWorkersInstrumentation)
                {
                    config.AddSource(Workers.Constants.ActivitySource.Name);
                }

                var otlpExporter = configuration.GetSection($"OpenTelemetry:Exporters:Otlp:Tracing");

                if (otlpExporter.Exists())
                {
                    config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();
                        
                        var headers = otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>();
                        if (headers is { Count: > 0 })
                        {
                            x.Headers = string.Join(",", headers.Select(y => $"{y.Key}={y.Value}"));
                        }
                    });
                }
            });

        return services;
    }
}

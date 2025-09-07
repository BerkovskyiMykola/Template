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
    public static IServiceCollection AddConfiguredOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _ = services.AddOpenTelemetry()
            .ConfigureResource(config =>
            {
                var serviceName = configuration["OpenTelemetry:ResourceAttributes:service.name"]!;
                var serviceInstanceId = configuration["OpenTelemetry:ResourceAttributes:service.instance.id"];

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
                var addRuntimeInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddRuntimeInstrumentation").Get<bool>();
                var addHttpClientInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddHttpClientInstrumentation").Get<bool>();
                var addAspNetCoreInstrumentation = configuration.GetSection("OpenTelemetry:Metrics:AddAspNetCoreInstrumentation").Get<bool>();

                if (addRuntimeInstrumentation)
                {
                    _ = config.AddRuntimeInstrumentation();
                }

                if (addHttpClientInstrumentation)
                {
                    _ = config.AddHttpClientInstrumentation();
                }

                if (addAspNetCoreInstrumentation)
                {
                    _ = config.AddAspNetCoreInstrumentation();
                }

                IConfigurationSection otlpExporter = configuration.GetSection("OpenTelemetry:Exporters:Otlp:Metrics");

                if (otlpExporter.Exists())
                {
                    _ = config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();

                        Dictionary<string, string>? headers = otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>();
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
                    _ = config.SetSampler<AlwaysOnSampler>();
                }

                if (addHttpClientInstrumentation)
                {
                    _ = config.AddHttpClientInstrumentation();
                }

                if (addAspNetCoreInstrumentation)
                {
                    _ = config.AddAspNetCoreInstrumentation();
                }

                if (addEntityFrameworkCoreInstrumentation)
                {
                    _ = config.AddEntityFrameworkCoreInstrumentation();
                }

                if (addWorkersInstrumentation)
                {
                    _ = config.AddSource(Constants.WorkersActivitySource.Name);
                }

                IConfigurationSection otlpExporter = configuration.GetSection($"OpenTelemetry:Exporters:Otlp:Tracing");

                if (otlpExporter.Exists())
                {
                    _ = config.AddOtlpExporter(x =>
                    {
                        x.Endpoint = new Uri(otlpExporter["Endpoint"]!);
                        x.Protocol = otlpExporter.GetSection("Protocol").Get<OtlpExportProtocol>();

                        Dictionary<string, string>? headers = otlpExporter.GetSection("Headers").Get<Dictionary<string, string>>();
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

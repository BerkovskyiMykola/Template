using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Template.Api.Common.OpenTelemetry;

/// <summary>  
/// Provides extension methods for OpenTelemetry in the application.  
/// </summary>  
internal static class OpenTelemetryExtensions
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
                var metricsAddRuntimeInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddRuntimeInstrumentation").Get<bool>();
                var metricsAddHttpClientInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddHttpClientInstrumentation").Get<bool>();
                var metricsAddAspNetCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddAspNetCoreInstrumentation").Get<bool>();

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

                var otlpEndpoint = configuration["OpenTelemetry:Exporters:Otlp:Endpoint"];

                if (otlpEndpoint is not null)
                {
                    config.AddOtlpExporter(x => x.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithTracing(config =>
            {
                var tracingAddHttpClientInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddHttpClientInstrumentation").Get<bool>();
                var tracingAddAspNetCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddAspNetCoreInstrumentation").Get<bool>();
                var tracingAddEntityFrameworkCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddEntityFrameworkCoreInstrumentation").Get<bool>();

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

                var otlpEndpoint = configuration["OpenTelemetry:Exporters:Otlp:Endpoint"];

                if (otlpEndpoint is not null)
                {
                    config.AddOtlpExporter(x => x.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}

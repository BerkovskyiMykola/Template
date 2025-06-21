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
        var serviceName = configuration["OpenTelemetry:ResourceAttributes:service.name"];

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentNullException(nameof(serviceName), "Service name must be configured in OpenTelemetry settings.");
        }

        var serviceInstanceId = configuration["OpenTelemetry:ResourceAttributes:service.instance.id"];

        if (string.IsNullOrWhiteSpace(serviceInstanceId))
        {
            throw new ArgumentNullException(nameof(serviceInstanceId), "Service instance Id must be configured in OpenTelemetry settings.");
        }

        var metricsAddRuntimeInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddRuntimeInstrumentation").Get<bool>();
        var metricsAddHttpClientInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddHttpClientInstrumentation").Get<bool>();
        var metricsAddAspNetCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Metrics:AddAspNetCoreInstrumentation").Get<bool>();
        var tracingAddHttpClientInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddHttpClientInstrumentation").Get<bool>();
        var tracingAddAspNetCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddAspNetCoreInstrumentation").Get<bool>();
        var tracingAddEntityFrameworkCoreInstrumentation = configuration.GetRequiredSection("OpenTelemetry:Tracing:AddEntityFrameworkCoreInstrumentation").Get<bool>();

        services.AddOpenTelemetry()
            .ConfigureResource(configuration =>
            {
                configuration
                    .AddService(
                        serviceName: serviceName,
                        serviceInstanceId: serviceInstanceId)
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            })
            .WithMetrics(configuration =>
            {
                if (metricsAddRuntimeInstrumentation)
                {
                    configuration.AddRuntimeInstrumentation();
                }

                if (metricsAddHttpClientInstrumentation)
                {
                    configuration.AddHttpClientInstrumentation();
                }

                if (metricsAddAspNetCoreInstrumentation)
                {
                    configuration.AddAspNetCoreInstrumentation();
                }
            })
            .WithTracing(configuration =>
            {
                if (environment.IsDevelopment())
                {
                    configuration.SetSampler<AlwaysOnSampler>();
                }

                if (tracingAddHttpClientInstrumentation)
                {
                    configuration.AddHttpClientInstrumentation();
                }

                if (tracingAddAspNetCoreInstrumentation)
                {
                    configuration.AddAspNetCoreInstrumentation();
                }

                if (tracingAddEntityFrameworkCoreInstrumentation)
                {
                    configuration.AddEntityFrameworkCoreInstrumentation();
                }
            });

        var otlpEndpoint = configuration["OpenTelemetry:Exporters:Otlp:Endpoint"];

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            services.ConfigureOpenTelemetryMeterProvider(
                configuration => configuration.AddOtlpExporter(x => x.Endpoint = new Uri(otlpEndpoint)));
            services.ConfigureOpenTelemetryTracerProvider(
                configuration => configuration.AddOtlpExporter(x => x.Endpoint = new Uri(otlpEndpoint)));
        }

        return services;
    }
}

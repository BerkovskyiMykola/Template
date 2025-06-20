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
        var applicationName = configuration["ApplicationName"]
            ?? throw new ArgumentNullException("applicationName", "Unable to determine the application name from configurations.");
        var serviceInstanceId = configuration["ServiceInstanceId"]
            ?? throw new ArgumentNullException("serviceInstanceId", "Unable to determine the service instance id from configurations.");

        services.AddOpenTelemetry()
            .ConfigureResource(configuration =>
            {
                configuration
                    .AddService(
                        serviceName: applicationName,
                        serviceInstanceId: serviceInstanceId)
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk();
            })
            .WithMetrics(configuration =>
            {
                configuration
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();
            })
            .WithTracing(configuration =>
            {
                if (environment.IsDevelopment())
                {
                    configuration.SetSampler<AlwaysOnSampler>();
                }

                configuration
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        var useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            services.ConfigureOpenTelemetryMeterProvider(configuration => configuration.AddOtlpExporter());
            services.ConfigureOpenTelemetryTracerProvider(configuration => configuration.AddOtlpExporter());
        }

        return services;
    }
}

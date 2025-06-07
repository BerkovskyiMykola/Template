using Serilog;
using Serilog.Core;
using Template.Api.Common.Serilog.Enrichers;

namespace Template.Api.Common.Serilog;

/// <summary>
/// Provides extension methods for configuring Serilog in the application.
/// </summary>
internal static class SerilogExtensions
{
    /// <summary>
    /// Configures and adds Serilog to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which Serilog will be added.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>  
    public static void AddConfiguredSerilog(this IServiceCollection services)
    {
        services.AddSingleton<ILogEventEnricher, AppNameEnricher>();

        services.AddSerilog((services, configuration) =>
        {
            configuration.ReadFrom.Configuration(services.GetRequiredService<IConfiguration>());
            configuration.ReadFrom.Services(services);
        });
    }
}

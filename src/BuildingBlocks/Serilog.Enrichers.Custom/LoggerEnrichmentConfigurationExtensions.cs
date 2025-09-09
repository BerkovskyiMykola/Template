using Serilog.Configuration;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Provides extension methods for configuring the <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Adds a <see cref="UserIdEnricher"/> to the <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="LoggerEnrichmentConfiguration"/> to which a <see cref="UserIdEnricher"/> will be added.</param>
    /// <returns>The <see cref="LoggerConfiguration"/> that includes a <see cref="UserIdEnricher"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>  
    public static LoggerConfiguration WithUserId(this LoggerEnrichmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.With<UserIdEnricher>();
    }
}

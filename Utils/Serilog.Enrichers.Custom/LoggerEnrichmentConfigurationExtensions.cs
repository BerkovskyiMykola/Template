using Serilog.Configuration;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Provides extension methods for configuring custom enrichers in Serilog.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Adds a UserId enricher to the logger configuration.
    /// </summary>
    /// <param name="config">The logger enrichment configuration.</param>
    /// <returns>The updated logger configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>  
    public static LoggerConfiguration WithUserId(this LoggerEnrichmentConfiguration config)
    {
        return config.With<UserIdEnricher>();
    }
}

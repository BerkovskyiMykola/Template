using Serilog.Configuration;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Provides extension methods for configuring custom enrichers in Serilog.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Adds a <see cref="CustomEnricher"/> to the logger configuration.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <returns>The logger configuration with <see cref="CustomEnricher"/> added.</returns>
    public static LoggerConfiguration WithCustom(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CustomEnricher>();
    }
}

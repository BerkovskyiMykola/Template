using Serilog.Configuration;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Provides extension methods for configuring custom enrichers in Serilog.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// The default header name for correlation Id.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>
    /// Adds a TraceId enricher to the logger configuration.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <returns>The updated logger configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enrichmentConfiguration"/> is null.</exception>  
    public static LoggerConfiguration WithTraceIdentifier(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<TraceIdentifierEnricher>();
    }

    /// <summary>
    /// Adds a CorrelationId enricher to the logger configuration.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <param name="headerName">The name of the header to use for the correlation Id. Defaults to "X-Correlation-Id".</param>
    /// <returns>The updated logger configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enrichmentConfiguration"/> or <paramref name="headerName"/> is null.</exception>  
    /// <exception cref="ArgumentException">Thrown if <paramref name="headerName"/> is empty or whitespace.</exception>  
    public static LoggerConfiguration WithCorrelationIdentifier(this LoggerEnrichmentConfiguration enrichmentConfiguration, string headerName = CorrelationIdHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        return enrichmentConfiguration.With(new CorrelationIdentifierEnricher(headerName));
    }

    /// <summary>
    /// Adds a UserId enricher to the logger configuration.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <returns>The updated logger configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enrichmentConfiguration"/> is null.</exception>  
    public static LoggerConfiguration WithUserIdentifier(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<UserIdentifierEnricher>();
    }
}

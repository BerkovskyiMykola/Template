/*
 * Serilog.Enrichers.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Serilog.Configuration;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Provides extension methods for configuring the <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Adds a <see cref="HttpContextNameIdClaimEnricher"/> to the <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="LoggerEnrichmentConfiguration"/> to which a <see cref="HttpContextNameIdClaimEnricher"/> will be added.</param>
    /// <returns>The <see cref="LoggerConfiguration"/> that includes a <see cref="HttpContextNameIdClaimEnricher"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> is null.</exception>  
    public static LoggerConfiguration WithHttpContextNameIdClaim(this LoggerEnrichmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.With<HttpContextNameIdClaimEnricher>();
    }
}

/*
 * Serilog.Enrichers.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// An <see cref="ILogEventEnricher"/> implementation that adds the value of a first <see cref="Claim"/> 
/// with the type <see cref="ClaimTypes.NameIdentifier"/> to the <see cref="LogEvent"/> as a property named "HttpContext.NameId"
/// if absent and if available in the current <see cref="HttpContext"/>.
/// </summary>
internal sealed class HttpContextNameIdClaimEnricher : ILogEventEnricher
{
    private readonly HttpContextAccessor _httpContextAccessor = new();

    /// <summary>
    /// Enriches the <paramref name="logEvent"/> with the the value of a first <see cref="Claim"/> 
    /// of type <see cref="ClaimTypes.NameIdentifier"/> as a property named "HttpContext.NameId" 
    /// if absent and if available in the current <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="logEvent">
    /// The <see cref="LogEvent"/> to enrich with the value of a first <see cref="Claim"/> 
    /// with the type <see cref="ClaimTypes.NameIdentifier"/> as a property named "HttpContext.NameId" 
    /// if absent and if available in the current <see cref="HttpContext"/>.
    /// </param>
    /// <param name="propertyFactory">
    /// The <see cref="ILogEventPropertyFactory"/> to create <see cref="LogEventProperty"/> instances.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="logEvent"/> or <paramref name="propertyFactory"/> is null.
    /// </exception>  
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        string? value = _httpContextAccessor.HttpContext?
            .User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (value is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("HttpContext.NameId", value));
    }
}

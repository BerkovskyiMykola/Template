using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Enricher that adds a TraceId if absent to the log event based on the current HTTP context.
/// </summary>
internal sealed class TraceIdentifierEnricher : ILogEventEnricher
{
    private readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    /// <summary>
    /// Enriches the log event with the TraceId if absent and if available in the HTTP context.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory to create log event properties.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logEvent"/> or <paramref name="propertyFactory"/> is null.</exception>  
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        var traceId = httpContext.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(traceId))
        {
            return;
        }

        var property = propertyFactory.CreateProperty("TraceIdentifier", traceId);
        logEvent.AddPropertyIfAbsent(property);
    }
}

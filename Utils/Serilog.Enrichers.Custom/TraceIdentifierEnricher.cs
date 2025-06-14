using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.Custom;

/// <summary>
/// Enricher that adds the TraceId if absent from the current HTTP context to the log event.
/// </summary>
internal sealed class TraceIdentifierEnricher : ILogEventEnricher
{
    private readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    /// <summary>
    /// Enriches the log event with the TraceId if absent and if an HTTP context is available.
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

        var property = propertyFactory.CreateProperty("TraceIdentifier", httpContext.TraceIdentifier);
        logEvent.AddPropertyIfAbsent(property);
    }
}

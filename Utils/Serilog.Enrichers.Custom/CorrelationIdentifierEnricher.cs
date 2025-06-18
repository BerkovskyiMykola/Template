using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.Custom;

/// <summary>  
/// Enricher that adds a CorrelationId if absent to log events based on a specified HTTP header.  
/// </summary>  
internal sealed class CorrelationIdentifierEnricher : ILogEventEnricher
{
    private readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
    private readonly string _headerName;

    /// <summary>  
    /// Initializes a new instance of the <see cref="CorrelationIdentifierEnricher"/> class.  
    /// </summary>  
    /// <param name="headerName">The HTTP header name used to retrieve the CorrelationId.</param>  
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerName"/> is null.</exception>  
    /// <exception cref="ArgumentException">Thrown if <paramref name="headerName"/> is empty or whitespace.</exception>  
    public CorrelationIdentifierEnricher(string headerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        _headerName = headerName;
    }

    /// <summary>  
    /// Enriches the log event with a CorrelationId if absent and if available in the HTTP context.  
    /// </summary>  
    /// <param name="logEvent">The log event to enrich.</param>  
    /// <param name="propertyFactory">The factory used to create log event properties.</param>  
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logEvent"/> or <paramref name="propertyFactory"/> is null.</exception>  
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        var correlationId = httpContext.Request.Headers[_headerName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        var property = propertyFactory.CreateProperty("CorrelationIdentifier", correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}

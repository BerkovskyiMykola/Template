using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.Custom;

/// <summary>  
/// A custom log event enricher that adds a "User" property to log events.  
/// </summary>  
internal sealed class CustomEnricher : ILogEventEnricher
{
    private readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    /// <summary>  
    /// Enriches the log event with a "User" property.  
    /// </summary>  
    /// <param name="logEvent">The log event to enrich.</param>  
    /// <param name="propertyFactory">The property factory used to create log event properties.</param>  
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        var user = "system-user";
        var property = propertyFactory.CreateProperty("User", user);
        logEvent.AddOrUpdateProperty(property);
    }
}

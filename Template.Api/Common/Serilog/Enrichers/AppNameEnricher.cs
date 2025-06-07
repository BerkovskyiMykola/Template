using Serilog.Core;
using Serilog.Events;

namespace Template.Api.Common.Serilog.Enrichers;

/// <summary>  
/// Enricher to add the application name to Serilog log events.  
/// </summary>  
internal sealed class AppNameEnricher : ILogEventEnricher
{
    /// <summary>  
    /// Enriches the log event with the application name property.  
    /// </summary>  
    /// <param name="logEvent">The log event to enrich.</param>  
    /// <param name="propertyFactory">The property factory to create log event properties.</param>  
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEvent"/> or <paramref name="propertyFactory"/> is null.</exception>  
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("AppName", nameof(Template)));
    }
}

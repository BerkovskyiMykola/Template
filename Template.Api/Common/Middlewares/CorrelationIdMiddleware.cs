using Serilog.Enrichers.Custom;

namespace Template.Api.Common.Middlewares;

/// <summary>
/// Middleware to ensure every request has a Correlation Id.
/// </summary>
internal sealed class CorrelationIdMiddleware : IMiddleware
{
    /// <summary>
    /// Invokes the middleware to check and set the Correlation Id header.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="next"/> is null.</exception>  
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue(LoggerEnrichmentConfigurationExtensions.CorrelationIdHeader, out var correlationIds) || 
            string.IsNullOrWhiteSpace(correlationIds.FirstOrDefault()))
        {
            context.Request.Headers[LoggerEnrichmentConfigurationExtensions.CorrelationIdHeader] = Ulid.NewUlid().ToString();
        }

        await next(context);
    }
}


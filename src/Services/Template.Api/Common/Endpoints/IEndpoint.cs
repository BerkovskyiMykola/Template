namespace Template.Api.Common.Endpoints;

/// <summary>
/// Defines a contract for endpoint mapping.
/// </summary>
internal interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define HTTP endpoint.</param>
    static abstract void MapEndpoint(IEndpointRouteBuilder builder);
}

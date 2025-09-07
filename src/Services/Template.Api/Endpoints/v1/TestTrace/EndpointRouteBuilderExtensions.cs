namespace Template.Api.Endpoints.v1.TestTrace;

/// <summary>
/// Extension methods for mapping TestTrace endpoints to the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all TestTrace endpoints to the provided <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define TestTrace HTTP endpoints.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> that includes all TestTrace mapped endpoints.</returns>

    public static IEndpointRouteBuilder MapTestTraceEndpoints(this IEndpointRouteBuilder builder)
    {
        Get.Endpoint.MapEndpoint(builder);
        Post.Endpoint.MapEndpoint(builder);

        return builder;
    }
}

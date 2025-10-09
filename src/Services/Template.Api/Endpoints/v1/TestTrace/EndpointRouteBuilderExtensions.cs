/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Template.Api.Endpoints.v1.TestTrace;

/// <summary>
/// Extension methods for mapping TestTrace-related HTTP endpoints to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all TestTrace endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define TestTrace HTTP endpoints.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> instance that includes all TestTrace mapped endpoints.</returns>
    internal static IEndpointRouteBuilder MapTestTraceEndpoints(this IEndpointRouteBuilder builder)
    {
        Get.Endpoint.MapEndpoint(builder);
        Post.Endpoint.MapEndpoint(builder);

        return builder;
    }
}

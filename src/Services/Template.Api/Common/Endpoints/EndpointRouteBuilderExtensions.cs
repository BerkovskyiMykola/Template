/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Template.Api.Endpoints.v1.TestTrace;

namespace Template.Api.Common.Endpoints;

/// <summary>
/// Extension methods for mapping endpoints to the <see cref="IEndpointRouteBuilder"/>.
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all endpoints to the provided <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define HTTP endpoints.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> that includes all mapped endpoints.</returns>
    internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder
            .MapTestTraceEndpoints();
    }
}

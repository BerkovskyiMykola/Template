/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Template.Api.Endpoints.v1.TestTrace;

namespace Template.Api.Common.Endpoints;

/// <summary>
/// Provides extension methods for mapping application-specific endpoints to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all registered endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define HTTP endpoints.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> with all mapped endpoints.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder
            .MapTestTraceEndpoints();
    }
}

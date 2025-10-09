/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Template.Api.Common.Endpoints;

/// <summary>
/// Represents a contract for mapping an HTTP endpoint to an <see cref="IEndpointRouteBuilder"/>.
/// Implementing types define how a specific endpoint should be mapped.
/// </summary>
internal interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to define HTTP endpoint.</param>
    static abstract void MapEndpoint(IEndpointRouteBuilder builder);
}

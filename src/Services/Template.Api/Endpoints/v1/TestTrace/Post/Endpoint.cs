/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.AspNetCore.Mvc;
using Template.Api.Common.Endpoints;

namespace Template.Api.Endpoints.v1.TestTrace.Post;

internal sealed class Endpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder) => _ = builder.MapPost("api/v1/test-trace", Handle);

    private static IResult Handle([FromBody] Request request) => Results.Ok(new Response(request.Name));
}

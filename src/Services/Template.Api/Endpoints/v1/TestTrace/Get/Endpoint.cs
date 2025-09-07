using Microsoft.AspNetCore.Mvc;
using Template.Api.Common.Endpoints;

namespace Template.Api.Endpoints.v1.TestTrace.Get;

internal sealed class Endpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder) => _ = builder.MapGet("/", HandleAsync);

    private static async Task<IResult> HandleAsync(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration)
    {
        System.Net.Http.HttpClient client = httpClientFactory.CreateClient(Common.HttpClients.ServiceCollectionExtensions.TestTraceNamedHttpClient);
        HttpResponseMessage response = await client.PostAsJsonAsync($"{configuration["ApiBaseAddress"]}/test-trace?test=test", new
        {
            Name = "Trace Name"
        });

        return Results.Ok($"Hello World! Test trace {await response.Content.ReadAsStringAsync()}");
    }
}

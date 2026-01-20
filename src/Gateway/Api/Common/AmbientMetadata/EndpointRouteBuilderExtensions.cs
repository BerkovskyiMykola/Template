/*
 * Api
 * Copyright (c) 2026-2026 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.AmbientMetadata;
using Microsoft.Extensions.Options;

namespace Api.Common.AmbientMetadata;

/// <summary>  
/// Extension methods for configuring ambient metadata in the endpoint route builder. 
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    private const string AmbientMetadataBaseRoute = "api/ambient-metadata";

    /// <summary>  
    /// Adds configured AmbientMetadata endpoints to the <see cref="IEndpointRouteBuilder"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="endpoints">The endpoint route builder to which the AmbientMetadata endpoints will be added.</param>  
    /// <returns>The original endpoint route builder.</returns> 
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoints"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required services are not registered in the dependency injection container.</exception>
    public static IEndpointRouteBuilder MapConfiguredAmbientMetadataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        #if DEBUG
        Guard.IsNotNull(endpoints);
        #endif

        if (endpoints.ServiceProvider.GetService(typeof(IOptions<BuildMetadata>)) is null ||
            endpoints.ServiceProvider.GetService(typeof(IOptions<ApplicationMetadata>)) is null)
        {
            throw new InvalidOperationException(
                $"Register {typeof(IOptions<BuildMetadata>)} and {typeof(IOptions<ApplicationMetadata>)} " +
                $"in the dependency injection container.");
        }

        endpoints.MapGet(
            $"{AmbientMetadataBaseRoute}/build", 
            static (IOptions<BuildMetadata> options) => options.Value);

        endpoints.MapGet(
            $"{AmbientMetadataBaseRoute}/application", 
            static (IOptions<ApplicationMetadata> options) => options.Value);

        return endpoints;
    }
}

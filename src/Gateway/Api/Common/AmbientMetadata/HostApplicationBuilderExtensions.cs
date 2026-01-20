/*
 * Api
 * Copyright (c) 2026-2026 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;

namespace Api.Common.AmbientMetadata;

/// <summary>  
/// Extension methods for configuring ambient metadata in the host application. 
/// </summary>
internal static class HostApplicationBuilderExtensions
{
    private const string AmbientMetadataSectionName = "AmbientMetadata";

    /// <summary>  
    /// Adds configured AmbientMetadata to the <see cref="IHostApplicationBuilder"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="builder">The host application to which the AmbientMetadata will be added.</param>  
    /// <returns>The original host application.</returns> 
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IHostApplicationBuilder AddConfiguredAmbientMetadata(this IHostApplicationBuilder builder)
    {
        #if DEBUG
        Guard.IsNotNull(builder);
        #endif

        _ = builder.Services.AddBuildMetadata(
            builder.Configuration.GetSection($"{AmbientMetadataSectionName}:Build"));

        _ = builder.UseApplicationMetadata(
            $"{AmbientMetadataSectionName}:Application");

        return builder;
    }
}

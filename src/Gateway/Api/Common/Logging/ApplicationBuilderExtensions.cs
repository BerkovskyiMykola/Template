/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;

namespace Api.Common.Logging;

/// <summary>  
/// Extension methods for configuring logging in the application. 
/// </summary>
internal static class ApplicationBuilderExtensions
{
    /// <summary>  
    /// Adds an HttpLogging to the <see cref="IApplicationBuilder"/> if the
    /// "HttpLogging" configuration section exists in the <see cref="IConfiguration"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="app">The application to which the HttpLogging will be added.</param>  
    /// <param name="configuration">The configuration used to check whether the HttpLogging is configured.</param>  
    /// <returns>The original host application.</returns> 
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or <paramref name="configuration"/> is null.</exception>
    public static IApplicationBuilder UseHttpLoggingIfConfigured(
        this IApplicationBuilder app, 
        IConfiguration configuration)
    {
        #if DEBUG
        Guard.IsNotNull(app);
        Guard.IsNotNull(configuration);
        #endif

        if (!configuration.GetSection("HttpLogging").Exists())
        {
            return app;
        }

        _ = app.UseHttpLogging();

        return app;
    }
}

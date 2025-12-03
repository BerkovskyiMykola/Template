/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using Logging.File.Custom;

namespace Api.Common.Logging;

/// <summary>  
/// Extension methods for configuring logging in the application. 
/// </summary>
internal static class LoggingBuilderExtensions
{
    /// <summary>  
    /// Adds a file logger named 'File' to the <see cref="ILoggingBuilder"/> if the
    /// "Logging:File" configuration section exists in the <see cref="IConfiguration"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="loggingBuilder">The builder to which the file logging provider will be added.</param>  
    /// <param name="configuration">The configuration used to check whether the file logging provider is configured.</param>  
    /// <returns>The original builder.</returns>  
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggingBuilder"/> or <paramref name="configuration"/> is null.</exception>
    public static ILoggingBuilder AddFileIfConfigured(
        this ILoggingBuilder loggingBuilder, 
        IConfiguration configuration)
    {
        #if DEBUG
        Guard.IsNotNull(loggingBuilder);
        Guard.IsNotNull(configuration);
        #endif

        if (configuration.GetSection("Logging:File").Exists())
        {
            _ = loggingBuilder.AddFile();
        }

        return loggingBuilder;
    }
}

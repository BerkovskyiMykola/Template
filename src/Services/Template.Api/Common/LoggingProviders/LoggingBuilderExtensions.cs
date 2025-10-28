/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Logging.File.Custom;

namespace Template.Api.Common.LoggingProviders;

/// <summary>  
/// Provides extension methods for configuring and adding logging providers to the application. 
/// </summary>
internal static class LoggingBuilderExtensions
{
    /// <summary>  
    /// Adds and configures logging providers, based on the <paramref name="configuration"/>, to the <paramref name="loggingBuilder"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="ILoggingBuilder"/> to add logging providers to.</param>  
    /// <param name="configuration">The <see cref="IConfiguration"/> containing logging providers settings.</param>  
    /// <returns>The <see cref="ILoggingBuilder"/> with providers configured.</returns>  
    public static ILoggingBuilder AddConfiguredProviders(
        this ILoggingBuilder loggingBuilder, 
        IConfiguration configuration)
    {
        if (configuration.GetSection("Logging:File").Exists())
        {
            _ = loggingBuilder.AddFile();
        }

        return loggingBuilder;
    }
}

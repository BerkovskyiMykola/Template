/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Serilog;

namespace Template.Api.Common.Serilog;

/// <summary>  
/// Provides extension methods for configuring and adding Serilog services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds and configures Serilog logging to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add configured Serilog to.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with Serilog configured.</returns>  
    internal static IServiceCollection AddConfiguredSerilog(this IServiceCollection services)
    {
        return services.AddSerilog(static (sp, config) =>
        {
            _ = config
                .ReadFrom.Configuration(sp.GetRequiredService<IConfiguration>())
                .ReadFrom.Services(sp);
        });
    }
}

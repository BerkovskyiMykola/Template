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
    public static IServiceCollection AddConfiguredSerilog(this IServiceCollection services)
    {
        services.AddSerilog((sp, config) =>
        {
            config.ReadFrom.Configuration(sp.GetRequiredService<IConfiguration>());
            config.ReadFrom.Services(sp);
        });

        return services;
    }
}

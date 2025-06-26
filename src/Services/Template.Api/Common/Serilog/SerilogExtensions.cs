using Serilog;

namespace Template.Api.Common.Serilog;

/// <summary>  
/// Provides extension methods for Serilog in the application.  
/// </summary>  
internal static class SerilogExtensions
{
    /// <summary>  
    /// Adds and configures Serilog logging for the application.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to add Serilog to.</param>  
    /// <returns>The <see cref="IServiceCollection"/> with Serilog configured.</returns>  
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is null.</exception>  
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

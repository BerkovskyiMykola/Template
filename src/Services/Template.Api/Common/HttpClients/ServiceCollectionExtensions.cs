/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Template.Api.Common.HttpClients;

/// <summary>  
/// Provides extension methods for configuring and adding <see cref="HttpClient"/> services in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// The name of the <see cref="HttpClient"/> instance used in <see cref="AddConfiguredTestTraceNamedHttpClient"/>.
    /// </summary>
    public const string TestTraceNamedHttpClient = "TestTrace";

    /// <summary>  
    /// Adds and configures <see cref="HttpClient"/> service, using the <see cref="TestTraceNamedHttpClient"/> name to the <paramref name="services"/>.  
    /// </summary>  
    /// <param name="services">The <see cref="IServiceCollection"/> to which the configured <see cref="HttpClient"/> service will be added.</param>  
    /// <returns>The <see cref="IServiceCollection"/> instance with <see cref="HttpClient"/> service configured.</returns>  
    public static IServiceCollection AddConfiguredTestTraceNamedHttpClient(
        this IServiceCollection services)
    {
         _ = services.AddHttpClient(TestTraceNamedHttpClient)
            .AddStandardResilienceHandler();

        return services;
    }
}

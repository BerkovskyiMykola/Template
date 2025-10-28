/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Logging.File.Custom.Formatters;
using Logging.File.Custom.Formatters.Json;
using Logging.File.Custom.Formatters.Simple;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Logging.File.Custom;

/// <summary>
/// Provides extension methods for the <see cref="ILoggingBuilder"/> and <see cref="ILoggerProviderConfiguration{LoggerProvider}"/> classes.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds a file logger named 'File' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <returns>The updated <see cref="ILoggingBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddConfiguration();
        
        _ = builder
            .AddFileFormatter<JsonFormatter, JsonFormatterOptions, FormatterConfigureOptions>()
            .AddFileFormatter<SimpleFormatter, SimpleFormatterOptions, FormatterConfigureOptions>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<LoggerOptions, LoggerProvider>(builder.Services);

        return builder;
    }

    #pragma warning disable S4018
    private static ILoggingBuilder AddFileFormatter<TFormatter, TOptions, TConfigureOptions>(this ILoggingBuilder builder)
        where TOptions : FormatterOptions
        where TFormatter : Formatter
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<Formatter, TFormatter>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TOptions>, TConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, FileLoggerFormatterOptionsChangeTokenSource<TOptions>>());

        return builder;
    }
    #pragma warning restore S4018

    private sealed class FileLoggerFormatterOptionsChangeTokenSource<TOptions>(
        ILoggerProviderConfiguration<LoggerProvider> providerConfiguration) 
        : ConfigurationChangeTokenSource<TOptions>(Helper.GetFormatterOptionsSection(providerConfiguration))
        where TOptions : FormatterOptions
    {
    }
}

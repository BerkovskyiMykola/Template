/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;

namespace Logging.File.Custom;

/// <summary>
/// Provides helper methods for working <see cref="Microsoft.Extensions.Logging.ILogger"/> related objects.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Retrieves the configuration section that contains the formatter options for a <see cref="LoggerProvider"/>.
    /// </summary>
    /// <param name="providerConfiguration">
    /// The provider configuration from which to obtain the formatter options section.
    /// </param>
    /// <returns>
    /// An <see cref="IConfiguration"/> section representing the formatter options.
    /// </returns>
    public static IConfiguration GetFormatterOptionsSection(ILoggerProviderConfiguration<LoggerProvider> providerConfiguration) 
        => providerConfiguration.Configuration.GetSection("FormatterOptions");
}

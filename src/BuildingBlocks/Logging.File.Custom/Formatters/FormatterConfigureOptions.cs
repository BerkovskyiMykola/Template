/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Logging.File.Custom.Formatters;

/// <summary>
/// Configures a <see cref="FormatterOptions"/> object from an <see cref="IConfiguration"/> section named "FormatterOptions".
/// </summary>
/// <param name="providerConfiguration">
/// The <see cref="ILoggerProviderConfiguration{TProvider}"/> for <see cref="LoggerProvider"/>.
/// The <see cref="IConfiguration"/> instance used to obtain the "FormatterOptions" configuration section is retrieved from this parameter.
/// </param>
internal sealed class FormatterConfigureOptions(
    ILoggerProviderConfiguration<LoggerProvider> providerConfiguration) : IConfigureOptions<FormatterOptions>
{
    private readonly IConfiguration _configuration = Helper.GetFormatterOptionsSection(providerConfiguration);

    /// <summary>
    /// Binds values from the "FormatterOptions" configuration section to the specified <see cref="FormatterOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="FormatterOptions"/> instance to configure.</param>
    /// <remarks>
    /// The method uses <see cref="ConfigurationBinder.Bind(IConfiguration, object)"/>
    /// to copy matching configuration values into the options object. The configuration section is obtained from the
    /// <see cref="ILoggerProviderConfiguration{TProvider}.Configuration"/> passed to the class.
    /// </remarks>
    public void Configure(FormatterOptions options) => _configuration.Bind(options);
}

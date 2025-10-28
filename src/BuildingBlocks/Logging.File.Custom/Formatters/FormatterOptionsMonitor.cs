/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.Options;

namespace Logging.File.Custom.Formatters;

/// <summary>
/// A <see cref="IOptionsMonitor{TOptions}"/> implementation that
/// always returns a single provided options instance.
/// </summary>
/// <typeparam name="TOptions">The type of the formatter options. Must derive from <see cref="FormatterOptions"/>.</typeparam>
/// <param name="options">The options instance that will be exposed as the current value.</param>
internal sealed class FormatterOptionsMonitor<TOptions>(TOptions options) :
    IOptionsMonitor<TOptions>
    where TOptions : FormatterOptions
{
    /// <summary>
    /// Gets the current options instance.
    /// </summary>
    /// <param name="name">The optional name of the options instance. This implementation ignores the name.</param>
    /// <returns>The currently held <typeparamref name="TOptions"/> instance.</returns>
    public TOptions Get(string? name) => CurrentValue;

    /// <summary>
    /// Registers a listener to be notified when the options change.
    /// </summary>
    /// <param name="listener">The callback invoked when the options change. This implementation does not support change notifications.</param>
    /// <returns>
    /// Always <see langword="null"/> because change notifications are not supported by this monitor.
    /// </returns>
    public IDisposable? OnChange(Action<TOptions, string> listener) => null;

    /// <summary>
    /// The current options instance exposed by this monitor.
    /// </summary>
    /// <remarks>
    /// This value is initialized from the constructor parameter and is immutable for
    /// the lifetime of the monitor.
    /// </remarks>
    public TOptions CurrentValue { get; } = options;
}

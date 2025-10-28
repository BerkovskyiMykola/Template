/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Logging.File.Custom.Formatters;
using Logging.File.Custom.Formatters.Json;
using Logging.File.Custom.Formatters.Simple;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logging.File.Custom;

/// <summary>
/// A provider of <see cref="Logger"/> instances.
/// </summary>
[ProviderAlias("File")]
internal sealed class LoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IOptionsMonitor<LoggerOptions> _options;
    private readonly ConcurrentDictionary<string, Logger> _loggers;
    private ConcurrentDictionary<string, Formatter> _formatters;
    private readonly LoggerProcessor _messageQueue;

    private readonly IDisposable? _optionsReloadToken;
    private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

    /// <summary>
    /// Creates an instance of <see cref="LoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to create <see cref="Logger"/> instances with.</param>
    public LoggerProvider(IOptionsMonitor<LoggerOptions> options)
        : this(options, []) { }

    /// <summary>
    /// Creates an instance of <see cref="LoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to create <see cref="Logger"/> instances with.</param>
    /// <param name="formatters">Log formatters added for <see cref="Logger"/> instances.</param>
    public LoggerProvider(IOptionsMonitor<LoggerOptions> options, IEnumerable<Formatter>? formatters)
    {
        _options = options;
        _loggers = new ConcurrentDictionary<string, Logger>();
        SetFormatters(formatters);
        _messageQueue = new LoggerProcessor(
            options.CurrentValue.QueueFullMode, 
            options.CurrentValue.MaxQueueLength,
            options.CurrentValue.Path,
            options.CurrentValue.RollingInterval);

        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        if (_options.CurrentValue.FormatterName is null || !_formatters.TryGetValue(_options.CurrentValue.FormatterName, out Formatter? logFormatter))
        {
            logFormatter = _formatters[FormatterNames.Simple];
        }

        return _loggers.TryGetValue(categoryName, out Logger? logger) 
            ? logger : _loggers.GetOrAdd(categoryName, new Logger(categoryName, _messageQueue, logFormatter, _scopeProvider, _options.CurrentValue));
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;

        foreach (Logger logger in _loggers.Values)
        {
            logger.ScopeProvider = _scopeProvider;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
        _messageQueue.Dispose();
    }

    private void ReloadLoggerOptions(LoggerOptions options)
    {
        if (options.FormatterName is null || !_formatters.TryGetValue(options.FormatterName, out Formatter? logFormatter))
        {
            logFormatter = _formatters[FormatterNames.Simple];
        }

        _messageQueue.UpdateQueueSettings(options.QueueFullMode, options.MaxQueueLength);
        _messageQueue.UpdateFileWriterSettings(options.Path, options.RollingInterval);

        foreach (Logger logger in _loggers.Values)
        {
            logger.Options = options;
            logger.Formatter = logFormatter;
        }
    }

    [MemberNotNull(nameof(_formatters))]
    private void SetFormatters(IEnumerable<Formatter>? formatters = null)
    {
        ConcurrentDictionary<string, Formatter> cd = new(StringComparer.OrdinalIgnoreCase);

        bool added = false;
        if (formatters is not null)
        {
            foreach (Formatter formatter in formatters)
            {
                _ = cd.TryAdd(formatter.Name, formatter);
                added = true;
            }
        }

        if (!added)
        {
            _ = cd.TryAdd(FormatterNames.Simple, new SimpleFormatter(new FormatterOptionsMonitor<SimpleFormatterOptions>(new SimpleFormatterOptions())));
            _ = cd.TryAdd(FormatterNames.Json, new JsonFormatter(new FormatterOptionsMonitor<JsonFormatterOptions>(new JsonFormatterOptions())));
        }

        _formatters = cd;
    }
}

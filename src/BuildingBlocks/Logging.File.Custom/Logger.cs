/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Text;
using Logging.File.Custom.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Logging.File.Custom;

/// <summary>
/// A logger that writes messages in the file.
/// </summary>
internal sealed class Logger(string name, LoggerProcessor loggerProcessor, Formatter formatter, IExternalScopeProvider? scopeProvider, LoggerOptions options) : ILogger, IBufferedLogger
{
    private const int StringBuilderMaxCapacity = 1024;

    private readonly string _name = name;
    private readonly LoggerProcessor _queueProcessor = loggerProcessor;

    /// <summary>
    /// Gets or sets the name of the .
    /// </summary>
    public Formatter Formatter { get; set; } = formatter;

    /// <summary>
    /// Gets or sets the name of the .
    /// </summary>
    public IExternalScopeProvider? ScopeProvider { get; set; } = scopeProvider;

    /// <summary>
    /// Gets or sets the .
    /// </summary>
    public LoggerOptions Options { get; set; } = options;

    [ThreadStatic]
    private static StringWriter? t_stringWriter;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        #pragma warning disable S2696
        t_stringWriter ??= new StringWriter();
        #pragma warning restore S2696

        LogEntry<TState> logEntry = new(logLevel, _name, eventId, state, exception, formatter);
        Formatter.Write(in logEntry, ScopeProvider, t_stringWriter);

        StringBuilder sb = t_stringWriter.GetStringBuilder();

        if (sb.Length is 0)
        {
            return;
        }

        string computedAnsiString = sb.ToString();

        _ = sb.Clear();

        if (sb.Capacity > StringBuilderMaxCapacity)
        {
            sb.Capacity = StringBuilderMaxCapacity;
        }

        _queueProcessor.EnqueueMessage(computedAnsiString);
    }

    /// <inheritdoc />
    public void LogRecords(IEnumerable<BufferedLogRecord> records)
    {
        #pragma warning disable S2696
        StringWriter writer = t_stringWriter ??= new StringWriter();
        #pragma warning restore S2696

        StringBuilder sb = writer.GetStringBuilder();
        foreach (BufferedLogRecord rec in records)
        {
            LogEntry<BufferedLogRecord> logEntry = new(rec.LogLevel, _name, rec.EventId, rec, null, static (s, _) => s.FormattedMessage ?? string.Empty);
            Formatter.Write(in logEntry, null, writer);

            if (sb.Length is 0)
            {
                continue;
            }

            string computedAnsiString = sb.ToString();
            _ = sb.Clear();

            _queueProcessor.EnqueueMessage(computedAnsiString);
        }

        if (sb.Capacity > StringBuilderMaxCapacity)
        {
            sb.Capacity = StringBuilderMaxCapacity;
        }
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel is not LogLevel.None;

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => ScopeProvider?.Push(state) ?? NullScope.Instance;
}

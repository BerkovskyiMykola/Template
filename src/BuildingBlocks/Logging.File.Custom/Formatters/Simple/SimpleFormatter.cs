/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Logging.File.Custom.Formatters.Simple;

//Copied from Microsoft.Extensions.Logging.Console

/// <summary>
/// Formats log entries as Simple.
/// </summary>
internal sealed class SimpleFormatter : Formatter, IDisposable
{
    private const string LogLevelPadding = ": ";
    private static readonly string _messagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LogLevelPadding.Length);
    private static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

    private readonly IDisposable? _optionsReloadToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFormatter"/>.
    /// </summary>
    /// <param name="options">The options monitor used to retrieve and listen for changes to <see cref="SimpleFormatterOptions"/>.</param>
    public SimpleFormatter(IOptionsMonitor<SimpleFormatterOptions> options)
        : base(FormatterNames.Simple)
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(SimpleFormatterOptions options) => FormatterOptions = options;

    /// <inheritdoc />
    public void Dispose() => _optionsReloadToken?.Dispose();

    /// <summary>
    /// Gets or sets the formatter options.
    /// </summary>
    public SimpleFormatterOptions FormatterOptions { get; set; }

    /// <inheritdoc />
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            string message = bufferedRecord.FormattedMessage ?? string.Empty;
            WriteInternal(
                null, 
                textWriter, 
                message, 
                bufferedRecord.LogLevel, 
                bufferedRecord.EventId.Id, 
                bufferedRecord.Exception, 
                logEntry.Category, 
                bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception is null && message is null)
            {
                return;
            }

            // We extract most of the work into a non-generic method to save code size. If this was left in the generic
            // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
            WriteInternal(
                scopeProvider, 
                textWriter,
                message, 
                logEntry.LogLevel, 
                logEntry.EventId.Id, 
                logEntry.Exception?.ToString(), 
                logEntry.Category, 
                GetCurrentDateTime());
        }
    }

    #pragma warning disable S107
    private void WriteInternal(
        IExternalScopeProvider? scopeProvider, 
        TextWriter textWriter, 
        string message, 
        LogLevel logLevel,
        int eventId, 
        string? exception, 
        string category, 
        DateTimeOffset stamp)
    {
        string logLevelString = GetLogLevelString(logLevel);

        string? timestamp = null;
        string? timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat is not null)
        {
            timestamp = stamp.ToString(timestampFormat, CultureInfo.InvariantCulture);
        }

        if (timestamp is not null)
        {
            textWriter.Write(timestamp);
        }

        if (logLevelString is not null)
        {
            textWriter.Write(logLevelString);
        }

        bool singleLine = FormatterOptions.SingleLine;

        // Example:
        // info: ConsoleApp.Program[10]
        //       Request received

        // category and event id
        textWriter.Write(LogLevelPadding);
        textWriter.Write(category);
        textWriter.Write('[');

        Span<char> span = stackalloc char[10];
        if (eventId.TryFormat(span, out int charsWritten, provider: CultureInfo.InvariantCulture))
        {
            textWriter.Write(span[..charsWritten]);
        }
        else
        {
            textWriter.Write(eventId.ToString(CultureInfo.InvariantCulture));
        }

        textWriter.Write(']');
        if (!singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }

        // scope information
        WriteScopeInformation(textWriter, scopeProvider, singleLine);
        WriteMessage(textWriter, message, singleLine);

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception is not null)
        {
            // exception message
            WriteMessage(textWriter, exception, singleLine);
        }

        if (singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }
    }
    #pragma warning restore S107

    private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (singleLine)
            {
                textWriter.Write(' ');
                WriteReplacing(textWriter, Environment.NewLine, " ", message);
            }
            else
            {
                textWriter.Write(_messagePadding);
                WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message);
                textWriter.Write(Environment.NewLine);
            }
        }

        static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
        {
            string newMessage = message.Replace(oldValue, newValue, StringComparison.Ordinal);
            writer.Write(newMessage);
        }
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        #pragma warning disable S6354, S3358
        return FormatterOptions.TimestampFormat is not null
            ? (FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
            : DateTimeOffset.MinValue;
        #pragma warning restore S6354, S3358
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider is not null)
        {
            bool paddingNeeded = !singleLine;
            scopeProvider.ForEachScope((scope, state) =>
            {
                if (paddingNeeded)
                {
                    paddingNeeded = false;
                    state.Write(_messagePadding);
                    state.Write("=> ");
                }
                else
                {
                    state.Write(" => ");
                }

                state.Write(scope);
            }, textWriter);

            if (!paddingNeeded && !singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }
    }
}

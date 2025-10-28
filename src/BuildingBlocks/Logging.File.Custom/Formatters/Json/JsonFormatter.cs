/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace Logging.File.Custom.Formatters.Json;

//Copied from Microsoft.Extensions.Logging.Console

/// <summary>
/// Formats log entries as JSON.
/// </summary>
internal sealed class JsonFormatter : Formatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormatter"/>.
    /// </summary>
    /// <param name="options">The options monitor used to retrieve and listen for changes to <see cref="JsonFormatterOptions"/>.</param>
    public JsonFormatter(IOptionsMonitor<JsonFormatterOptions> options)
        : base(FormatterNames.Json)
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

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
                logEntry.Category, 
                bufferedRecord.EventId.Id, 
                bufferedRecord.Exception,
                bufferedRecord.Attributes.Count > 0, 
                null, 
                bufferedRecord.Attributes, 
                bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception is null && message is null)
            {
                return;
            }

            #pragma warning disable S6354, S3358
            DateTimeOffset stamp = FormatterOptions.TimestampFormat is not null
                ? (FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
                : DateTimeOffset.MinValue;
            #pragma warning restore S6354, S3358

            // We extract most of the work into a non-generic method to save code size. If this was left in the generic
            // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
            WriteInternal(
                scopeProvider, 
                textWriter, 
                message, 
                logEntry.LogLevel, 
                logEntry.Category, 
                logEntry.EventId.Id, 
                logEntry.Exception?.ToString(),
                logEntry.State is not null, 
                logEntry.State?.ToString(), 
                logEntry.State as IReadOnlyList<StringNullableObjectPair>, 
                stamp);
        }
    }

    #pragma warning disable S107
    private void WriteInternal(
        IExternalScopeProvider? scopeProvider, 
        TextWriter textWriter, 
        string? message, 
        LogLevel logLevel,
        string category, 
        int eventId, 
        string? exception, 
        bool hasState, 
        string? stateMessage, 
        IReadOnlyList<StringNullableObjectPair>? stateProperties,
        DateTimeOffset stamp)
    {
        const int DefaultBufferSize = 1024;
        using (PooledByteBufferWriter output = new(DefaultBufferSize))
        {
            using (Utf8JsonWriter writer = new(output, FormatterOptions.JsonWriterOptions))
            {
                writer.WriteStartObject();
                string? timestampFormat = FormatterOptions.TimestampFormat;

                if (timestampFormat is not null)
                {
                    writer.WriteString("Timestamp", stamp.ToString(timestampFormat, CultureInfo.InvariantCulture));
                }

                writer.WriteNumber(nameof(LogEntry<>.EventId), eventId);
                writer.WriteString(nameof(LogEntry<>.LogLevel), GetLogLevelString(logLevel));
                writer.WriteString(nameof(LogEntry<>.Category), category);
                writer.WriteString("Message", message);

                if (exception is not null)
                {
                    writer.WriteString(nameof(Exception), exception);
                }

                if (hasState)
                {
                    writer.WriteStartObject(nameof(LogEntry<>.State));
                    writer.WriteString("Message", stateMessage);
                    if (stateProperties is not null)
                    {
                        foreach (StringNullableObjectPair item in stateProperties)
                        {
                            WriteItem(writer, item);
                        }
                    }

                    writer.WriteEndObject();
                }

                WriteScopeInformation(writer, scopeProvider);
                writer.WriteEndObject();
                writer.Flush();
            }

            ReadOnlySpan<byte> messageBytes = output.WrittenMemory.Span;
            char[] logMessageBuffer = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(messageBytes.Length));
            try
            {
                int charsWritten = Encoding.UTF8.GetChars(messageBytes, logMessageBuffer);

                textWriter.Write(logMessageBuffer, 0, charsWritten);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(logMessageBuffer);
            }
        }

        textWriter.Write(Environment.NewLine);
    }
    #pragma warning restore S107

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "Trace",
            LogLevel.Debug => "Debug",
            LogLevel.Information => "Information",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Critical",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private void WriteScopeInformation(Utf8JsonWriter writer, IExternalScopeProvider? scopeProvider)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider is not null)
        {
            writer.WriteStartArray("Scopes");
            scopeProvider.ForEachScope(static (scope, state) =>
            {
                if (scope is IEnumerable<StringNullableObjectPair> scopeItems)
                {
                    state.WriteStartObject();
                    state.WriteString("Message", scope.ToString());
                    foreach (StringNullableObjectPair item in scopeItems)
                    {
                        WriteItem(state, item);
                    }

                    state.WriteEndObject();
                }
                else
                {
                    state.WriteStringValue(ToInvariantString(scope));
                }
            }, writer);
            writer.WriteEndArray();
        }
    }

    private static void WriteItem(Utf8JsonWriter writer, StringNullableObjectPair item)
    {
        string key = item.Key;
        switch (item.Value)
        {
            case bool boolValue:
                writer.WriteBoolean(key, boolValue);
                break;
            case byte byteValue:
                writer.WriteNumber(key, byteValue);
                break;
            case sbyte sbyteValue:
                writer.WriteNumber(key, sbyteValue);
                break;
            case char charValue:
                writer.WriteString(key, MemoryMarshal.CreateSpan(ref charValue, 1));
                break;
            case decimal decimalValue:
                writer.WriteNumber(key, decimalValue);
                break;
            case double doubleValue:
                writer.WriteNumber(key, doubleValue);
                break;
            case float floatValue:
                writer.WriteNumber(key, floatValue);
                break;
            case int intValue:
                writer.WriteNumber(key, intValue);
                break;
            case uint uintValue:
                writer.WriteNumber(key, uintValue);
                break;
            case long longValue:
                writer.WriteNumber(key, longValue);
                break;
            case ulong ulongValue:
                writer.WriteNumber(key, ulongValue);
                break;
            case short shortValue:
                writer.WriteNumber(key, shortValue);
                break;
            case ushort ushortValue:
                writer.WriteNumber(key, ushortValue);
                break;
            case null:
                writer.WriteNull(key);
                break;
            default:
                writer.WriteString(key, ToInvariantString(item.Value));
                break;
        }
    }

    private static string? ToInvariantString(object? obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets or sets the formatter options.
    /// </summary>
    public JsonFormatterOptions FormatterOptions { get; set; }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(JsonFormatterOptions options) => FormatterOptions = options;

    /// <inheritdoc />
    public void Dispose() => _optionsReloadToken?.Dispose();

    #pragma warning disable
    private sealed class PooledByteBufferWriter : PipeWriter, IDisposable
    {
        // This class allows two possible configurations: if rentedBuffer is not null then
        // it can be used as an IBufferWriter and holds a buffer that should eventually be
        // returned to the shared pool. If rentedBuffer is null, then the instance is in a
        // cleared/disposed state and it must re-rent a buffer before it can be used again.
        private byte[]? _rentedBuffer;
        private int _index;
        private readonly Stream? _stream;

        private const int MinimumBufferSize = 256;

        // Value copied from Array.MaxLength in System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Array.cs.
        public const int MaximumBufferSize = 0X7FFFFFC7;

        private PooledByteBufferWriter()
        {
            // Ensure we are in sync with the Array.MaxLength implementation.
            Debug.Assert(MaximumBufferSize == Array.MaxLength);
        }

        public PooledByteBufferWriter(int initialCapacity) : this()
        {
            Debug.Assert(initialCapacity > 0);

            _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _index = 0;
        }

        public PooledByteBufferWriter(int initialCapacity, Stream stream) : this(initialCapacity)
        {
            _stream = stream;
        }

        public ReadOnlyMemory<byte> WrittenMemory
        {
            get
            {
                Debug.Assert(_rentedBuffer != null);
                Debug.Assert(_index <= _rentedBuffer.Length);
                return _rentedBuffer.AsMemory(0, _index);
            }
        }

        public int WrittenCount
        {
            get
            {
                Debug.Assert(_rentedBuffer != null);
                return _index;
            }
        }

        public int Capacity
        {
            get
            {
                Debug.Assert(_rentedBuffer != null);
                return _rentedBuffer.Length;
            }
        }

        public int FreeCapacity
        {
            get
            {
                Debug.Assert(_rentedBuffer != null);
                return _rentedBuffer.Length - _index;
            }
        }

        public void Clear()
        {
            ClearHelper();
        }

        public void ClearAndReturnBuffers()
        {
            Debug.Assert(_rentedBuffer != null);

            ClearHelper();
            byte[] toReturn = _rentedBuffer;
            _rentedBuffer = null;
            ArrayPool<byte>.Shared.Return(toReturn);
        }

        private void ClearHelper()
        {
            Debug.Assert(_rentedBuffer != null);
            Debug.Assert(_index <= _rentedBuffer.Length);

            _rentedBuffer.AsSpan(0, _index).Clear();
            _index = 0;
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (_rentedBuffer == null)
            {
                return;
            }

            ClearHelper();
            byte[] toReturn = _rentedBuffer;
            _rentedBuffer = null;
            ArrayPool<byte>.Shared.Return(toReturn);
        }

        public void InitializeEmptyInstance(int initialCapacity)
        {
            Debug.Assert(initialCapacity > 0);
            Debug.Assert(_rentedBuffer is null);

            _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _index = 0;
        }

        public static PooledByteBufferWriter CreateEmptyInstanceForCaching() => new PooledByteBufferWriter();

        public override void Advance(int count)
        {
            Debug.Assert(_rentedBuffer != null);
            Debug.Assert(count >= 0);
            Debug.Assert(_index <= _rentedBuffer.Length - count);
            _index += count;
        }

        public override Memory<byte> GetMemory(int sizeHint = MinimumBufferSize)
        {
            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsMemory(_index);
        }

        public override Span<byte> GetSpan(int sizeHint = MinimumBufferSize)
        {
            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsSpan(_index);
        }

        public void WriteToStream(Stream destination)
        {
            destination.Write(WrittenMemory.Span);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            Debug.Assert(_rentedBuffer != null);
            Debug.Assert(sizeHint > 0);

            int currentLength = _rentedBuffer.Length;
            int availableSpace = currentLength - _index;

            // If we've reached ~1GB written, grow to the maximum buffer
            // length to avoid incessant minimal growths causing perf issues.
            if (_index >= MaximumBufferSize / 2)
            {
                sizeHint = Math.Max(sizeHint, MaximumBufferSize - currentLength);
            }

            if (sizeHint > availableSpace)
            {
                int growBy = Math.Max(sizeHint, currentLength);

                int newSize = currentLength + growBy;

                if ((uint)newSize > MaximumBufferSize)
                {
                    newSize = currentLength + sizeHint;
                    if ((uint)newSize > MaximumBufferSize)
                    {
                        throw new OutOfMemoryException($"Cannot allocate a buffer larger than {MaximumBufferSize} bytes.");
                    }
                }

                byte[] oldBuffer = _rentedBuffer;

                _rentedBuffer = ArrayPool<byte>.Shared.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= _index);
                Debug.Assert(_rentedBuffer.Length >= _index);

                Span<byte> oldBufferAsSpan = oldBuffer.AsSpan(0, _index);
                oldBufferAsSpan.CopyTo(_rentedBuffer);
                oldBufferAsSpan.Clear();
                ArrayPool<byte>.Shared.Return(oldBuffer);
            }

            Debug.Assert(_rentedBuffer.Length - _index > 0);
            Debug.Assert(_rentedBuffer.Length - _index >= sizeHint);
        }

        public override async ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(_stream is not null);
            await _stream.WriteAsync(WrittenMemory, cancellationToken).ConfigureAwait(false);
            Clear();

            return new FlushResult(isCanceled: false, isCompleted: false);
        }

        public override bool CanGetUnflushedBytes => true;
        public override long UnflushedBytes => _index;

        // This type is used internally in JsonSerializer to help buffer and flush bytes to the underlying Stream.
        // It's only pretending to be a PipeWriter and doesn't need Complete or CancelPendingFlush for the internal usage.
        public override void CancelPendingFlush() => throw new NotImplementedException();
        public override void Complete(Exception? exception = null) => throw new NotImplementedException();
    }
    #pragma warning restore
}

/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using static HttpClient.Logger.Custom.MediaTypeOptions;
using LogField = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Provides helper methods for working <see cref="System.Net.Http.HttpClient"/> related objects.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Adds the <paramref name="headers"/> to the <paramref name="log"/>, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </summary>
    /// <param name="log">
    /// The collection of parameters to which <paramref name="headers"/> will be added, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </param>
    /// <param name="headers">
    /// The <see cref="HttpHeaders"/> to add into the <paramref name="log"/>, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </param>
    /// <param name="allowedHeaders">
    /// A readonly set of allowed header names. <paramref name="headers"/> not in this set will be redacted.
    /// </param>
    public static void AddAllowedOrRedactedHeadersToLog(
        ICollection<LogField> log,
        HttpHeaders headers,
        IReadOnlySet<string> allowedHeaders)
    {
        const string Redacted = "[Redacted]";

        foreach ((string key, IEnumerable<string> value) in headers)
        {
            log.Add(allowedHeaders.Contains(key) ? new(key, string.Join(',', value)) : new(key, Redacted));
        }
    }

    /// <summary>
    /// Formats the provided <paramref name="log"/> as a string, starting with the given <paramref name="title"/> 
    /// followed by each key-value pair on a new line.
    /// </summary>
    /// <param name="title">
    /// The title to prepend to the log output.
    /// </param>
    /// <param name="log">
    /// The list of key-value pairs representing log entries to format.
    /// </param>
    /// <returns>
    /// A formatted string that starts with the <paramref name="title"/>, followed by each key-value pair in <paramref name="log"/> on a new line.
    /// </returns>
    public static string FormatLog(string title, IReadOnlyList<LogField> log)
    {
        // Currently using ValueStringBuilder (copied from System.Text).
        // This can be replaced with StringBuilder from ObjectPool
        // (see example: https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-9.0).
        // This avoids maintaining copied code and allows reusing StringBuilder instances via pooling.
        // Use 2kb as a rough average size for request/response headers
        using ValueStringBuilder builder = new(2 * 1024);
        int count = log.Count;
        builder.Append(title);
        builder.Append(":");
        builder.Append(Environment.NewLine);

        for (int i = 0; i < count - 1; i++)
        {
            LogField logField = log[i];
            builder.Append(logField.Key);
            builder.Append(": ");
            builder.Append(logField.Value?.ToString());
            builder.Append(Environment.NewLine);
        }

        if (count > 0)
        {
            LogField logField = log[count - 1];
            builder.Append(logField.Key);
            builder.Append(": ");
            builder.Append(logField.Value?.ToString());
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reads the <paramref name="content"/> as a string using the provided <paramref name="encoding"/>, up to a maximum of <paramref name="logLimit"/> bytes.
    /// </summary>
    /// <param name="content">
    /// The <see cref="HttpContent"/> to read from. The method attempts to read its content 
    /// and convert it into a string using the provided <paramref name="encoding"/> and reads up to a maximum of <paramref name="logLimit"/> bytes.
    /// </param>
    /// <param name="encoding">
    /// The <see cref="Encoding"/> to use when decoding the bytes from the <paramref name="content"/>.
    /// </param>
    /// <param name="logLimit">
    /// The maximum number of bytes to read from the <paramref name="content"/>. If the content length 
    /// exceeds this <paramref name="logLimit"/>, only the first <paramref name="logLimit"/> bytes will be read and decoded.
    /// </param>
    /// <param name="logger">
    /// The <see cref="ILogger"/> used to log decoding failures, specifically if a <see cref="DecoderFallbackException"/> occurs.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> representation of the <paramref name="content"/> if readable within the specified <paramref name="logLimit"/>. 
    /// If the <paramref name="content"/> is empty, returns <c>null</c>. 
    /// If decoding fails due to a <see cref="DecoderFallbackException"/>, returns the string <c>"&lt;Decoder failure&gt;"</c>.
    /// </returns>
    public static async Task<string?> ReadContentAsStringOrDefaultAsync(
        HttpContent content,
        Encoding encoding,
        long logLimit,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);

        using Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        if (stream.Length is 0)
        {
            return null;
        }

        int bufferSize = (int)Math.Min(stream.Length, logLimit);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);

            try
            {
                return encoding.GetString(buffer, 0, bytesRead);
            }
            catch (DecoderFallbackException ex)
            {
                logger.LogBodyDecodeFailureAsDebug(ex);
                return "<Decoder failure>";
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static readonly IReadOnlyList<Encoding> SupportedEncodings =
    [
        Encoding.UTF8,
        Encoding.Unicode,
        Encoding.ASCII,
        Encoding.Latin1
    ];

    /// <summary>
    /// Attempts to determine the appropriate <see cref="Encoding"/> for the specified <paramref name="contentType"/> 
    /// by matching it against a list of known <paramref name="mediaTypes"/>.
    /// </summary>
    /// <param name="contentType">
    /// The content type string to evaluate (e.g., "application/json; charset=utf-8").
    /// </param>
    /// <param name="mediaTypes">
    /// A list of <see cref="MediaTypeState"/> objects representing supported media types and their default encodings.
    /// </param>
    /// <param name="encoding">
    /// When this method returns, contains the <see cref="Encoding"/> to use for the <paramref name="contentType"/> if a match is found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a supported encoding is found for the specified <paramref name="contentType"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetEncodingForMediaType(string? contentType, IReadOnlyCollection<MediaTypeState> mediaTypes, [NotNullWhen(true)] out Encoding? encoding)
    {
        encoding = null;

        if (string.IsNullOrWhiteSpace(contentType) ||
            mediaTypes.Count is 0 ||
            !Microsoft.Net.Http.Headers.MediaTypeHeaderValue.TryParse(contentType, out Microsoft.Net.Http.Headers.MediaTypeHeaderValue? mediaType))
        {
            return false;
        }

        foreach (MediaTypeState state in mediaTypes)
        {
            if (!state.MediaTypeHeaderValue.MatchesMediaType(mediaType.MediaType))
            {
                continue;
            }

            encoding = mediaType.Encoding;
            if (encoding is null)
            {
                // No encoding specified, use the default.
                encoding = state.Encoding;
                return true;
            }

            // Only allow specific encodings.
            for (int i = 0; i < SupportedEncodings.Count; i++)
            {
                if (string.Equals(encoding.WebName,
                    SupportedEncodings[i].WebName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    encoding = SupportedEncodings[i];
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified flagged enum <paramref name="value"/> is valid.
    /// </summary>
    /// <typeparam name="T">
    /// The enum type to validate. Must be a struct and an <see cref="Enum"/>.
    /// </typeparam>
    /// <param name="value">
    /// The flagged enum value to validate.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is a valid combination of defined enum values; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsFlaggedEnumValid<T>(T value) where T : struct, Enum
    {
        long longValue = Convert.ToInt64(value, null);
        long mask = 0;
        foreach (T enumValue in Enum.GetValues<T>())
        {
            long enumValueAsInt64 = Convert.ToInt64(enumValue, null);
            if ((enumValueAsInt64 & longValue) == enumValueAsInt64)
            {
                mask |= enumValueAsInt64;
                if (mask == longValue)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //Copied from System.Text
    #pragma warning disable
    private ref struct ValueStringBuilder
    {
        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        public int Length
        {
            get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
            }
        }

        public int Capacity => _chars.Length;

        public void EnsureCapacity(int capacity)
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
                Grow(capacity - _pos);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
        /// the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
        }

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars => _chars;

        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
        public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
        }

        public void Insert(int index, string? s)
        {
            if (s == null)
            {
                return;
            }

            int count = s.Length;

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s.CopyTo(_chars.Slice(index));
            _pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            Span<char> chars = _chars;
            if ((uint)pos < (uint)chars.Length)
            {
                chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string? s)
        {
            if (s == null)
            {
                return;
            }

            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }

            s.CopyTo(_chars.Slice(pos));
            _pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            Span<char> dst = _chars.Slice(_pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            _pos += count;
        }

        public void Append(scoped ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = _pos;
            if (origPos > _chars.Length - length)
            {
                Grow(length);
            }

            _pos = origPos + length;
            return _chars.Slice(origPos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        /// <summary>
        /// Resize the internal buffer either by doubling current buffer size or
        /// by adding <paramref name="additionalCapacityBeyondPos"/> to
        /// <see cref="_pos"/> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        /// Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

            // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
            // to double the size if possible, bounding the doubling to not go beyond the max array length.
            int newCapacity = (int)Math.Max(
                (uint)(_pos + additionalCapacityBeyondPos),
                Math.Min((uint)_chars.Length * 2, ArrayMaxLength));

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
            // This could also go negative if the actual required length wraps around.
            char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }
    }
    #pragma warning restore
}

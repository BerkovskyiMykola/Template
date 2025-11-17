/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
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
}

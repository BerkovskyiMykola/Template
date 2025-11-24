/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using static HttpClient.Logger.Custom.MediaTypeOptions;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Helper methods for working with <see cref="System.Net.Http.HttpClient"/> related objects.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Adds the <paramref name="headers"/> to the <paramref name="keyValuePairs"/>, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </summary>
    /// <param name="keyValuePairs">
    /// The collection of parameters to which <paramref name="headers"/> will be added, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </param>
    /// <param name="headers">
    /// The headers to add into the <paramref name="keyValuePairs"/>, redacting those not included in the <paramref name="allowedHeaders"/>.
    /// </param>
    /// <param name="allowedHeaders">
    /// A readonly set of allowed header names. <paramref name="headers"/> not in this set will be redacted.
    /// </param>
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyValuePairs"/>, <paramref name="headers"/> or <paramref name="allowedHeaders"/> is null.</exception>
    public static void AddAllowedOrRedactedHeadersToCollection(
        ICollection<StringNullableObjectPair> keyValuePairs,
        HttpHeaders headers,
        IReadOnlySet<string> allowedHeaders)
    {
        #if DEBUG
        Guard.IsNotNull(keyValuePairs);
        Guard.IsNotNull(headers);
        Guard.IsNotNull(allowedHeaders);
        #endif

        foreach ((string key, IEnumerable<string> value) in headers)
        {
            keyValuePairs.Add(allowedHeaders.Contains(key)
                ? new(key, string.Join(',', value))
                : new(key, "[Redacted]"));
        }
    }

    /// <summary>
    /// Reads the <paramref name="content"/> as a string using the provided <paramref name="encoding"/>, up to a maximum of <paramref name="logLimit"/> bytes.
    /// </summary>
    /// <param name="content">
    /// The content to read from and convert it into a string using the provided <paramref name="encoding"/> and reads up to a maximum of <paramref name="logLimit"/> bytes.
    /// </param>
    /// <param name="encoding">
    /// The encoding to use when decoding the bytes from the <paramref name="content"/>.
    /// </param>
    /// <param name="logLimit">
    /// The maximum number of bytes to read from the <paramref name="content"/>. If the content length 
    /// exceeds this <paramref name="logLimit"/>, only the first <paramref name="logLimit"/> bytes will be read and decoded.
    /// </param>
    /// <param name="logger">
    /// The logger used to log decoding failures, specifically if a <see cref="DecoderFallbackException"/> occurs.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> representation of the <paramref name="content"/> if readable within the specified <paramref name="logLimit"/>. 
    /// If the <paramref name="content"/> is empty, returns <c>null</c>. 
    /// If decoding fails due to a <see cref="DecoderFallbackException"/>, returns the string <c>"&lt;Decoder failure&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/>, <paramref name="encoding"/> or <paramref name="logger"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="logLimit"/> is less than 0.</exception>
    public static async Task<string?> ReadContentAsStringOrDefaultAsync(
        HttpContent content,
        Encoding encoding,
        long logLimit,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        #if DEBUG
        Guard.IsNotNull(content);
        Guard.IsNotNull(encoding);
        Guard.IsGreaterThanOrEqualTo(logLimit, 0);
        Guard.IsNotNull(logger);
        #endif

        using Stream stream = await content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        if (stream.Length is 0)
        {
            return null;
        }

        int bufferSize = (int)Math.Min(stream.Length, logLimit);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int bytesRead = await stream
                .ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken)
                .ConfigureAwait(false);

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
    /// A list of objects representing supported media types and their default encodings.
    /// </param>
    /// <param name="encoding">
    /// When this method returns, contains the encoding to use for the <paramref name="contentType"/> if a match is found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a supported encoding is found for the specified <paramref name="contentType"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaTypes"/> is null.</exception>
    public static bool TryGetEncodingForMediaType(
        string? contentType,
        IReadOnlyCollection<MediaTypeState> mediaTypes,
        [NotNullWhen(true)] out Encoding? encoding)
    {
        #if DEBUG
        Guard.IsNotNull(mediaTypes);
        #endif

        encoding = null;

        if (string.IsNullOrWhiteSpace(contentType) ||
            mediaTypes.Count is 0 ||
            !Microsoft.Net.Http.Headers.MediaTypeHeaderValue.TryParse(
                contentType, out Microsoft.Net.Http.Headers.MediaTypeHeaderValue? mediaType))
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
}

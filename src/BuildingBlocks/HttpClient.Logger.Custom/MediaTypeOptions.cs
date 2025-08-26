using System.Text;
using Microsoft.Net.Http.Headers;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Options for <see cref="System.Net.Http.HttpClient"/> logging to configure which <see cref="Encoding"/> to use for each media type.
/// </summary>
public sealed class MediaTypeOptions
{
    /// <summary>
    /// The list of configured <see cref="MediaTypeState"/> instances.
    /// </summary>
    private readonly List<MediaTypeState> _mediaTypeStates = [];

    /// <summary>
    /// Gets the list of configured <see cref="MediaTypeState"/> instances.
    /// </summary>
    internal List<MediaTypeState> MediaTypeStates => _mediaTypeStates;

    /// <summary>
    /// Adds a <paramref name="mediaType"/> to be used for logging as text.
    /// If the <see cref="MediaTypeHeaderValue.Encoding"/> is not specified, <see cref="Encoding.UTF8"/> will be used by default.
    /// </summary>
    /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> to add.</param>
    private void AddText(MediaTypeHeaderValue mediaType)
    {
        mediaType.Encoding ??= Encoding.UTF8;

        _mediaTypeStates.Add(new MediaTypeState(mediaType, mediaType.Encoding));
    }

    /// <summary>
    /// Adds a <paramref name="contentType"/> to be used for logging as text.
    /// </summary>
    /// <remarks>
    /// If charset is not specified in the <paramref name="contentType"/>, the <see cref="Encoding"/> will default to <see cref="Encoding.UTF8"/>.
    /// </remarks>
    /// <param name="contentType">The content type to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentType"/> is null.</exception>  
    public void AddText(string contentType)
    {
        AddText(MediaTypeHeaderValue.Parse(contentType));
    }

    /// <summary>
    /// Adds a <paramref name="contentType"/> to be used for logging as text with a specific <paramref name="encoding"/>.
    /// </summary>
    /// <param name="contentType">The content type to add.</param>
    /// <param name="encoding">The <see cref="Encoding"/> to use for the <paramref name="contentType"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentType"/> or <paramref name="encoding"/> is null.</exception> 
    public void AddText(string contentType, Encoding encoding)
    {
        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        mediaType.Encoding = encoding;
        AddText(mediaType);
    }

    /// <summary>
    /// Clears all media types.
    /// </summary>
    public void Clear()
    {
        _mediaTypeStates.Clear();
    }

    /// <summary>
    /// Represents the state of a <see cref="Microsoft.Net.Http.Headers.MediaTypeHeaderValue"/>, including its <see cref="System.Text.Encoding"/>.
    /// </summary>
    /// <param name="MediaTypeHeaderValue">
    /// The <see cref="Microsoft.Net.Http.Headers.MediaTypeHeaderValue"/> that defines the media type.
    /// </param>
    /// <param name="Encoding">
    /// The <see cref="System.Text.Encoding"/> to use for content of this media type.
    /// </param>
    internal record MediaTypeState(
        MediaTypeHeaderValue MediaTypeHeaderValue, 
        Encoding Encoding);
}

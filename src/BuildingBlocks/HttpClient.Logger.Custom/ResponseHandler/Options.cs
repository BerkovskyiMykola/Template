namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// Options for the <see cref="Handler"/>.
/// </summary>
public sealed record Options
{
    /// <summary>
    /// Fields to log for the <see cref="HttpRequestMessage"/>.
    /// </summary>
    public LoggingFields LoggingFields { get; set; } = LoggingFields.None;

    /// <summary>
    /// <see cref="HttpResponseMessage.Headers"/> that are allowed to be logged.
    /// <para>
    /// If a header is not present in the <see cref="AllowedHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when trying to set a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public ISet<string> AllowedHeaders { get; set; } = new HashSet<string>();

    /// <summary>
    /// Options for configuring encodings for a specific <see cref="HttpResponseMessage.Content"/> media type.
    /// <para>
    /// If the <see cref="HttpResponseMessage.Content"/> do not match the supported media type,
    /// the <see cref="HttpResponseMessage.Content"/> will not be logged.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when trying to set a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public MediaTypeOptions AllowedMediaTypes { get; set; } = new();

    /// <summary>
    /// Maximum <see cref="HttpResponseMessage.Content"/> size to log (in bytes).
    /// </summary>
    public int BodyLogLimit { get; set; } = 0;
}

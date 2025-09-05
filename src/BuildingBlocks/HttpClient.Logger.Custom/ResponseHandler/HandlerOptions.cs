namespace HttpClient.Logger.Custom.ResponseHandler;

/// <summary>
/// Options for the <see cref="Handler"/>.
/// </summary>
public sealed class HandlerOptions
{
    private LoggingFields _loggingFields = LoggingFields.None;

    /// <summary>
    /// Fields to log for the <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the value assigned is not a valid <see cref="ResponseHandler.LoggingFields"/> enum.
    /// </exception>
    public LoggingFields LoggingFields
    {
        get => _loggingFields;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"{nameof(value)} ('{value}') must be a valid {nameof(ResponseHandler.LoggingFields)}.");
            }

            _loggingFields = value;
        }
    }

    /// <summary>
    /// <see cref="HttpResponseMessage.Headers"/> that are allowed to be logged.
    /// <para>
    /// If a header is not present in the <see cref="AllowedHeaders"/>,
    /// the header name will be logged with a redacted value.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public HashSet<string> AllowedHeaders { get; } = [];

    /// <summary>
    /// Options for configuring encodings for a specific <see cref="HttpResponseMessage.Content"/> media type.
    /// <para>
    /// If the <see cref="HttpResponseMessage.Content"/> do not match the supported media type,
    /// the <see cref="HttpResponseMessage.Content"/> will not be logged.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the getter is called when the value is null.</exception>
    public MediaTypeOptions AllowedMediaTypes { get; } = new();

    private int _bodyLogLimit;

    /// <summary>
    /// Maximum <see cref="HttpResponseMessage.Content"/> size to log (in bytes).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if a negative value is assigned.
    /// </exception>
    public int BodyLogLimit
    {
        get => _bodyLogLimit;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            _bodyLogLimit = value;
        }
    }
}

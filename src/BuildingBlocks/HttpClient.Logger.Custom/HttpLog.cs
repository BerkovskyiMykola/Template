using System.Collections;
using System.Text;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Represents a structured HTTP log with a title and a collection of key-value pairs.
/// </summary>
internal sealed class HttpLog : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly List<KeyValuePair<string, object?>> _keyValues;
    private readonly string _title;
    private string? _cachedToString;

    /// <summary>
    /// A callback function used for logging, which invokes <see cref="ToString"/> on the provided <see cref="HttpLog"/> state.
    /// </summary>
    internal static readonly Func<object, Exception?, string> Callback = (state, exception) => ((HttpLog)state).ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpLog"/> class.
    /// </summary>
    /// <param name="keyValues">The key-value pairs to be logged.</param>
    /// <param name="title">The title of the log entry.</param>
    public HttpLog(List<KeyValuePair<string, object?>> keyValues, string title)
    {
        _keyValues = keyValues;
        _title = title;
    }

    /// <summary>
    /// Gets the key-value pair at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    public KeyValuePair<string, object?> this[int index] => _keyValues[index];

    /// <summary>
    /// Gets the number of key-value pairs in the log.
    /// </summary>
    public int Count => _keyValues.Count;

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the key-value pairs.</returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        var count = _keyValues.Count;
        for (var i = 0; i < count; i++)
        {
            yield return _keyValues[i];
        }
    }

    /// <summary>
    /// Returns a string that represents the current log, formatted with the title and key-value pairs.
    /// Caches the result to avoid repeated string building.
    /// </summary>
    /// <returns>A string representation of the log.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resulting string is <c>null</c>.</exception>
    public override string ToString()
    {
        if (_cachedToString == null)
        {
            // Use 2kb as a rough average size for request/response headers
            var builder = new StringBuilder(2 * 1024);
            var count = _keyValues.Count;
            builder.Append(_title);
            builder.Append(':');
            builder.Append(Environment.NewLine);

            for (var i = 0; i < count - 1; i++)
            {
                var kvp = _keyValues[i];
                builder.Append(kvp.Key);
                builder.Append(": ");
                builder.Append(kvp.Value?.ToString());
                builder.Append(Environment.NewLine);
            }

            if (count > 0)
            {
                var kvp = _keyValues[count - 1];
                builder.Append(kvp.Key);
                builder.Append(": ");
                builder.Append(kvp.Value?.ToString());
            }

            _cachedToString = builder.ToString();
        }

        return _cachedToString;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection (non-generic implementation).
    /// </summary>
    /// <returns>An enumerator for the key-value pairs.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resulting enumerator is <c>null</c>.</exception>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

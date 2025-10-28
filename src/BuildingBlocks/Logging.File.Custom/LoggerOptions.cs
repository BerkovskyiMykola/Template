/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Logging.File.Custom;

#pragma warning disable S2325

/// <summary>
/// Options for a <see cref="Logger"/>.
/// </summary>
public sealed class LoggerOptions
{
    /// <summary>
    /// Gets or sets the name of the log message formatter to use.
    /// </summary>
    /// <value>
    /// The default value is <see langword="simple" />.
    /// </value>
    public string? FormatterName { get; set; }

    /// <summary>
    /// Gets or sets the desired file logger behavior when the queue becomes full.
    /// </summary>
    /// <value>
    /// The default value is <see langword="wait" />.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value assigned is not valid.
    /// </exception>
    public LoggerQueueFullMode QueueFullMode
    {
        get;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"{nameof(value)} ('{value}') must be a valid {nameof(LoggerQueueFullMode)}.");
            }

            field = value;
        }
    } = LoggerQueueFullMode.Wait;

    internal const int DefaultMaxQueueLengthValue = 2500;

    /// <summary>
    /// Gets or sets the maximum number of enqueued messages.
    /// </summary>
    /// <value>
    /// The default value is 2500.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value assigned is negative or zero.
    /// </exception>
    public int MaxQueueLength
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

            field = value;
        }
    } = DefaultMaxQueueLengthValue;

    /// <summary>
    /// Gets or sets the directory path where log files will be written.
    /// </summary>
    /// <value>
    /// The default value is "Logs".
    /// </value>
    /// <exception cref="ArgumentException">
    /// Thrown when the value assigned is null, empty, or consists only of white-space characters;
    /// or when value contains invalid path characters;
    /// or when value appears to contain a file (has a file extension).
    /// </exception>
    public string Path
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (value.ContainsAny(System.IO.Path.GetInvalidPathChars()))
            {
                throw new ArgumentException($"{nameof(Path)} contains invalid path characters.", nameof(value));
            }

            if (System.IO.Path.HasExtension(value))
            {
                throw new ArgumentException($"{nameof(Path)} must not contain a file.", nameof(value));
            }

            field = value;
        }
    } = "Logs";

    /// <summary>
    /// Gets or sets the interval at which log files should roll.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value assigned is not valid.
    /// </exception>
    public RollingInterval? RollingInterval
    {
        get;
        set
        {
            if (value.HasValue && !Enum.IsDefined(value.Value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"{nameof(value)} ('{value}') must be a valid {nameof(Custom.RollingInterval)}.");
            }

            field = value;
        }
    }
}

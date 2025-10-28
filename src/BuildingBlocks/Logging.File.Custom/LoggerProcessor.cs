/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

/*
PSEUDOCODE / PLAN (detailed):

- Purpose:
  - Add XML documentation to the existing LoggerProcessor implementation to explain intent,
    behavior, parameters, return values and exceptions. Keep implementation code unchanged.

- Steps:
  1. Add XML <summary> for the `LoggerProcessor` class describing responsibility and lifecycle.
  2. For each constructor and public method, add:
     - <summary> describing the high-level behavior.
     - <param> tags for parameters.
     - <returns> for methods that return values.
     - <remarks> where useful to explain concurrency or rolling file behavior.
     - <exception> when a known exception may be thrown (e.g., ArgumentOutOfRangeException).
  3. For internal/private helper methods add <summary> and <param>/<returns> as needed.
  4. Keep existing code and logic unchanged; only add XML documentation comments.
  5. Preserve existing file header and pragmas.
*/

using System.Globalization;

namespace Logging.File.Custom;

#pragma warning disable S108, S6566, S2221, CA1031, S6354, S6563

/// <summary>
/// Processes log messages for the file logger by queuing messages and writing them to disk.
/// </summary>
internal sealed class LoggerProcessor : IDisposable
{
    private int _maxQueueLength;
    private LoggerQueueFullMode _fullMode;
    private readonly Queue<string> _messageQueue;
    private volatile int _messagesDropped;
    private bool _isAddingCompleted;

    private string _path;
    private RollingInterval? _rollingInterval;
    private DateTime? _currentFilePeriod;
    private readonly Lock _writerLock = new();
    private StreamWriter _writer;

    private readonly Thread _outputThread;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerProcessor"/> class.
    /// </summary>
    /// <param name="fullMode">Behavior when the internal queue is full.</param>
    /// <param name="maxQueueLength">Maximum number of messages that may be queued.</param>
    /// <param name="path">Directory path where log files will be written. Directory will be created if missing.</param>
    /// <param name="rollingInterval">Optional rolling interval; when provided the logger will roll files by period.</param>
    public LoggerProcessor(LoggerQueueFullMode fullMode, int maxQueueLength, string path, RollingInterval? rollingInterval)
    {
        _messageQueue = new Queue<string>();
        _fullMode = fullMode;
        _maxQueueLength = maxQueueLength;

        _path = path;
        _rollingInterval = rollingInterval;

        _ = Directory.CreateDirectory(path);

        if (_rollingInterval is null)
        {
            _writer = CreateWriter(_path);
        }
        else
        {
            _currentFilePeriod = GetCurrentPeriod(_rollingInterval.Value, DateTime.Now);
            _writer = CreateWriterForPeriod(_path, _rollingInterval.Value, _currentFilePeriod.Value);
        }

        // Start File message queue processor
        _outputThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "File logger queue processing thread"
        };
        _outputThread.Start();
    }

    /// <summary>
    /// Updates queue behavior and maximum queue length at runtime.
    /// </summary>
    /// <param name="fullMode">New behavior when the queue becomes full.</param>
    /// <param name="maxQueueLength">New maximum queue length.</param>
    public void UpdateQueueSettings(LoggerQueueFullMode fullMode, int maxQueueLength)
    {
        lock (_messageQueue)
        {
            _fullMode = fullMode;
            _maxQueueLength = maxQueueLength;
            Monitor.PulseAll(_messageQueue);
        }
    }

    /// <summary>
    /// Updates file writer settings such as target directory and rolling interval.
    /// </summary>
    /// <param name="path">New directory path to store log files.</param>
    /// <param name="rollingInterval">New rolling interval. Pass <c>null</c> to disable rolling.</param>
    public void UpdateFileWriterSettings(string path, RollingInterval? rollingInterval)
    {
        lock (_writerLock)
        {
            _writer.Flush();
            _writer.Dispose();

            _path = path;
            _rollingInterval = rollingInterval;

            _ = Directory.CreateDirectory(path);

            if (_rollingInterval is null)
            {
                _writer = CreateWriter(_path);
                _currentFilePeriod = null;
            }
            else
            {
                _currentFilePeriod = GetCurrentPeriod(_rollingInterval.Value, DateTime.Now);
                _writer = CreateWriterForPeriod(_path, _rollingInterval.Value, _currentFilePeriod.Value);
            }
        }
    }

    /// <summary>
    /// Attempts to enqueue a message for asynchronous processing; if enqueuing is not possible
    /// because the processor has been completed, the message is written synchronously.
    /// </summary>
    /// <param name="message">Message to enqueue or write.</param>
    public void EnqueueMessage(string message)
    {
        // cannot enqueue when adding is completed
        if (!Enqueue(message))
        {
            WriteMessage(message);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CompleteAdding();

        try
        {
            _outputThread.Join();
        }
        catch (ThreadStateException) { }

        lock (_writerLock)
        {
            _writer.Flush();
            _writer.Dispose();
        }
    }

    private void ProcessLogQueue()
    {
        while (TryDequeue(out string? message))
        {
            WriteMessage(message);
        }
    }

    private void WriteMessage(string? entry)
    {
        try
        {
            lock (_writerLock)
            {
                if (_rollingInterval is not null)
                {
                    DateTime now = DateTime.Now;

                    if (now >= _currentFilePeriod!.Value.Add(GetTimeSpanForInterval(_rollingInterval.Value)))
                    {
                        _writer.Flush();
                        _writer.Dispose();

                        _currentFilePeriod = GetCurrentPeriod(_rollingInterval.Value, now);
                        _writer = CreateWriterForPeriod(_path, _rollingInterval.Value, _currentFilePeriod.Value);
                    }
                }

                _writer.Write(entry);
            }
        }
        catch
        {
            CompleteAdding();
        }
    }

    private bool Enqueue(string item)
    {
        lock (_messageQueue)
        {
            while (_messageQueue.Count >= _maxQueueLength && !_isAddingCompleted)
            {
                if (_fullMode == LoggerQueueFullMode.DropWrite)
                {
                    _messagesDropped++;
                    return true;
                }

                _ = Monitor.Wait(_messageQueue);
            }

            if (!_isAddingCompleted)
            {
                bool startedEmpty = _messageQueue.Count == 0;
                if (_messagesDropped > 0)
                {
                    _messageQueue.Enqueue($"Dropped {_messagesDropped} messages due to queue overflow.");

                    _messagesDropped = 0;
                }

                // if we just logged the dropped message warning this could push the queue size to
                // MaxLength + 1, that shouldn't be a problem. It won't grow any further until it is less than
                // MaxLength once again.
                _messageQueue.Enqueue(item);

                // if the queue started empty it could be at 1 or 2 now
                if (startedEmpty)
                {
                    // pulse for wait in Dequeue
                    Monitor.PulseAll(_messageQueue);
                }

                return true;
            }
        }

        return false;
    }

    private bool TryDequeue(out string? item)
    {
        lock (_messageQueue)
        {
            while (_messageQueue.Count == 0 && !_isAddingCompleted)
            {
                _ = Monitor.Wait(_messageQueue);
            }

            if (_messageQueue.Count > 0)
            {
                item = _messageQueue.Dequeue();
                if (_messageQueue.Count == _maxQueueLength - 1)
                {
                    // pulse for wait in Enqueue
                    Monitor.PulseAll(_messageQueue);
                }

                return true;
            }

            item = default;
            return false;
        }
    }

    private void CompleteAdding()
    {
        lock (_messageQueue)
        {
            _isAddingCompleted = true;
            Monitor.PulseAll(_messageQueue);
        }
    }

    private static StreamWriter CreateWriter(string path) 
        => new(Path.Combine(path, "logs.txt"), true);

    private static StreamWriter CreateWriterForPeriod(string path, RollingInterval interval, DateTime period) 
        => new(Path.Combine(path, $"logs-{period.ToString(GetFormat(interval), CultureInfo.InvariantCulture)}.txt"), true);

    private static DateTime GetCurrentPeriod(RollingInterval interval, DateTime now)
    {
        return interval switch
        {
            RollingInterval.Year => new DateTime(now.Year, 1, 1, 0, 0, 0, now.Kind),
            RollingInterval.Month => new DateTime(now.Year, now.Month, 1, 0, 0, 0, now.Kind),
            RollingInterval.Day => new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, now.Kind),
            RollingInterval.Hour => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Kind),
            RollingInterval.Minute => new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind),
            _ => throw new ArgumentOutOfRangeException(
                nameof(interval),
                interval,
                $"{nameof(interval)} ('{interval}') must be a valid {nameof(RollingInterval)}.")
        };
    }

    private static TimeSpan GetTimeSpanForInterval(RollingInterval interval)
    {
        return interval switch
        {
            RollingInterval.Year => TimeSpan.FromDays(365),
            RollingInterval.Month => TimeSpan.FromDays(30),
            RollingInterval.Day => TimeSpan.FromDays(1),
            RollingInterval.Hour => TimeSpan.FromHours(1),
            RollingInterval.Minute => TimeSpan.FromMinutes(1),
            _ => throw new ArgumentOutOfRangeException(
                nameof(interval),
                interval,
                $"{nameof(interval)} ('{interval}') must be a valid {nameof(RollingInterval)}.")
        };
    }

    private static string GetFormat(RollingInterval interval)
    {
        return interval switch
        {
            RollingInterval.Year => "yyyy",
            RollingInterval.Month => "yyyyMM",
            RollingInterval.Day => "yyyyMMdd",
            RollingInterval.Hour => "yyyyMMddHH",
            RollingInterval.Minute => "yyyyMMddHHmm",
            _ => throw new ArgumentOutOfRangeException(
                nameof(interval),
                interval,
                $"{nameof(interval)} ('{interval}') must be a valid {nameof(RollingInterval)}.")
        };
    }
}

/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.ObjectPool;

namespace HttpClient.Logger.Custom;

/// <summary>
/// Represents a pooled list of <see cref="LogField"/> objects that implements the <see cref="IResettable"/>.
/// </summary>
internal sealed class PooledLogFieldList : IResettable
{
    private const int DefaultCapacity = 16;
    private const int MaxRetainedCapacity = 128;

    /// <summary>
    /// Gets the list of <see cref="LogField"/> items.
    /// </summary>
    internal List<LogField> Items { get; } = new(DefaultCapacity);

    /// <summary>
    /// Attempts to reset the list for reuse. 
    /// Returns <c>true</c> if the list was cleared and can be reused; 
    /// <c>false</c> if the capacity exceeds the maximum retained capacity.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the list was cleared and can be reused; otherwise, <c>false</c>.
    /// </returns>
    public bool TryReset()
    {
        if (Items.Capacity > MaxRetainedCapacity)
        {
            return false;
        }

        Items.Clear();
        return true;
    }
}

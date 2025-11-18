/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.ObjectPool;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom;

/// <summary>
/// A pooled, reusable list of <see cref="StringNullableObjectPair"/> instances,
/// used to reduce allocations during logging.
/// </summary>
internal sealed class PooledStringNullableObjectPairList : IResettable
{
    private const int DefaultCapacity = 16;
    private const int MaxRetainedCapacity = 128;

    /// <summary>
    /// Gets the list of <see cref="StringNullableObjectPair"/> items.
    /// </summary>
    public List<StringNullableObjectPair> Items { get; } = new(DefaultCapacity);

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

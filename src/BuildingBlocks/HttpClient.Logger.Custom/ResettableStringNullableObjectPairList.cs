/*
 * HttpClient.Logger.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using Microsoft.Extensions.ObjectPool;
using StringNullableObjectPair = System.Collections.Generic.KeyValuePair<string, object?>;

namespace HttpClient.Logger.Custom;

/// <summary>
/// A reusable list of string - nullable object pairs, used to reduce allocations during logging.
/// </summary>
internal sealed class ResettableStringNullableObjectPairList 
    : List<StringNullableObjectPair>, IResettable
{
    private const int DefaultCapacity = 16;
    private const int MaxRetainedCapacity = 128;

    /// <inheritdoc/>
    public ResettableStringNullableObjectPairList() : base(DefaultCapacity) { }

    /// <inheritdoc/>
    public bool TryReset()
    {
        if (Capacity > MaxRetainedCapacity)
        {
            return false;
        }

        Clear();

        return true;
    }
}

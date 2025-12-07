/*
 * CommunityToolkit.Diagnostics.Extensions
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Runtime.CompilerServices;

namespace CommunityToolkit.Diagnostics.Extensions;

public static partial class GuardExt
{
    /// <summary>
    /// Asserts that the input value is a Url.
    /// </summary>
    /// <param name="value">The input value to test.</param>
    /// <param name="paramName">The name of the input parameter being tested.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not a URL.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsUrl(
        string value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        #pragma warning disable CA1062
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        #pragma warning restore CA1062
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ThrowHelperExt.ThrowArgumentExceptionForIsUrl(
            value,
            paramName);
    }
}

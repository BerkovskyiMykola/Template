/*
 * CommunityToolkit.Diagnostics.Extensions
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Runtime.CompilerServices;

namespace CommunityToolkit.Diagnostics.Extensions;

#pragma warning disable CA1711

public partial class GuardEx
{
    /// <summary>
    /// Asserts that the input value is defined <see langword="enum"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type of <see langword="enum"/> value type being tested.</typeparam>
    /// <param name="value">The input value to test.</param>
    /// <param name="paramName">The name of the input parameter being tested.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not defined <see langword="enum"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsDefinedEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
    {
        if (Enum.IsDefined(value))
        {
            return;
        }

        ThrowHelperEx.ThrowArgumentOutOfRangeExceptionForIsDefinedEnum(value, paramName);
    }

    /// <summary>
    /// Asserts that the input value is defined flags <see langword="enum"/> combination.
    /// </summary>
    /// <typeparam name="TEnum">The type of <see langword="enum"/> value type being tested.</typeparam>
    /// <param name="value">The input value to test.</param>
    /// <param name="paramName">The name of the input parameter being tested.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is not defined flags <see langword="enum"/> combination.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsDefinedFlagsEnumCombination<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where TEnum : struct, Enum
    {
        long valueAsInt64 = Convert.ToInt64(value, null);
        long mask = 0;
        foreach (TEnum enumValue in Enum.GetValues<TEnum>())
        {
            long enumValueAsInt64 = Convert.ToInt64(enumValue, null);
            if ((enumValueAsInt64 & valueAsInt64) == enumValueAsInt64)
            {
                mask |= enumValueAsInt64;
                if (mask == valueAsInt64)
                {
                    return;
                }
            }
        }

        ThrowHelperEx.ThrowArgumentOutOfRangeExceptionForIsDefinedFlagsEnumCombination(value, paramName);
    }
}

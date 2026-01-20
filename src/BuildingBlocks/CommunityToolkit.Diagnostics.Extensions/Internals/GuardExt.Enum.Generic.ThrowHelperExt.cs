/*
 * CommunityToolkit.Diagnostics.Extensions
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics.CodeAnalysis;

namespace CommunityToolkit.Diagnostics.Extensions;

public static partial class GuardExt
{
    private static partial class ThrowHelperExt
    {
        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeExceptionForIsDefinedEnum<TEnum>(
            TEnum actualValue,
            string? paramName)
            where TEnum : struct, Enum
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                actualValue,
                $"Parameter \"{paramName}\" ({typeof(TEnum).Name}) must be defined enum.");
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeExceptionForIsDefinedFlagsEnumCombination<TEnum>(
            TEnum actualValue,
            string? paramName)
            where TEnum : struct, Enum
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                actualValue,
                $"Parameter \"{paramName}\" ({typeof(TEnum).Name}) must be defined flags enum combination.");
        }
    }
}

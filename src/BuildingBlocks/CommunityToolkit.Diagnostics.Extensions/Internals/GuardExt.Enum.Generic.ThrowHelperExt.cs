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
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when <see cref="IsDefinedEnum{TEnum}(TEnum,string)"/> fails.
        /// </summary>
        /// <typeparam name="TEnum">The type of the input actual value.</typeparam>
        /// <param name="actualValue">The argument actual value.</param>
        /// <param name="paramName">The argument name.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown with the specified parameters.</exception>
        /// <returns>This method always throws, so it actually never returns a value.</returns>
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

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when <see cref="IsDefinedFlagsEnumCombination{TEnum}(TEnum,string)"/> fails.
        /// </summary>
        /// <typeparam name="TEnum">The type of the input actual value.</typeparam>
        /// <param name="actualValue">The argument actual value.</param>
        /// <param name="paramName">The argument name.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown with the specified parameters.</exception>
        /// <returns>This method always throws, so it actually never returns a value.</returns>
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

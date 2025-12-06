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
        /// Throws an <see cref="ArgumentException"/> when <see cref="IsUrl(string,string)"/> fails.
        /// </summary>
        /// <param name="value">The argument value.</param>
        /// <param name="paramName">The argument name.</param>
        /// <exception cref="ArgumentException">Thrown with the specified parameters.</exception>
        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsUrl(
            string value,
            string? paramName)
        {
            throw new ArgumentException(
                $"Parameter {AssertString(paramName)} (string) must be a Url, was {AssertString(value)}.", 
                paramName);
        }
    }
}

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

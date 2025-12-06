/*
 * CommunityToolkit.Diagnostics.Extensions
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;

namespace CommunityToolkit.Diagnostics.Extensions;

public static partial class GuardExt
{
    [StackTraceHidden]
    private static partial class ThrowHelperExt
    {
        private static string AssertString(object? obj)
        {
            return obj switch
            {
                string _ => $"\"{obj}\"",
                null => "null",
                _ => $"<{obj}>"
            };
        }
    }
}

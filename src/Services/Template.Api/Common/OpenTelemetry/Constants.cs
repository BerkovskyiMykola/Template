/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;

namespace Template.Api.Common.OpenTelemetry;

/// <summary>
/// Provides application-wide constants and shared OpenTelemetry resources.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Gets the <see cref="ActivitySource"/> used for tracing activities within worker services.
    /// </summary>
    public static ActivitySource WorkersActivitySource { get; } = new ActivitySource("Workers");
}

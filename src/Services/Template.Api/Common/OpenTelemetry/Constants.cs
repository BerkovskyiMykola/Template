/*
 * Template.Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using System.Diagnostics;

namespace Template.Api.Common.OpenTelemetry;

/// <summary>
/// Provides application-wide constant values and shared resources for OpenTelemetry services.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The <see cref="ActivitySource"/> used for tracing activities within worker services.
    /// </summary>
    internal static ActivitySource WorkersActivitySource { get; } = new ActivitySource("Workers");
}

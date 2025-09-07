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
    public static ActivitySource WorkersActivitySource { get; } = new ActivitySource("Workers");
}

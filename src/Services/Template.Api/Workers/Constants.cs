using System.Diagnostics;

namespace Template.Api.Workers;

/// <summary>
/// Provides application-wide constant values and shared resources for worker components.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The <see cref="System.Diagnostics.ActivitySource"/> used for tracing activities within worker services.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new ActivitySource("Workers");
}

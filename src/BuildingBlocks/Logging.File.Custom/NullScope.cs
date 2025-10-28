/*
 * Logging.File.Custom
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

namespace Logging.File.Custom;

/// <summary>
/// An empty scope without any logic
/// </summary>
internal sealed class NullScope : IDisposable
{
    /// <summary>
    /// Returns a cached instance of <see cref="NullScope"/>.
    /// </summary>
    public static NullScope Instance { get; } = new NullScope();

    private NullScope()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}

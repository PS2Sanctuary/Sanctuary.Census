using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a cache of data.
/// </summary>
public interface IDataCacheService
{
    /// <summary>
    /// Gets the time in UTC at which the cache was last populated.
    /// </summary>
    DateTimeOffset LastPopulated { get; }

    /// <summary>
    /// Repopulates the cache.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Repopulate(CancellationToken ct = default);
}

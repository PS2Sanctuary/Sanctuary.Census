﻿using System;
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
    /// Returns <see cref="DateTimeOffset.MinValue"/> if the cache is not populated.
    /// </summary>
    DateTimeOffset LastPopulated { get; }

    /// <summary>
    /// Repopulates the cache.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RepopulateAsync(CancellationToken ct = default);

    /// <summary>
    /// Clears the cache.
    /// </summary>
    void Clear();
}

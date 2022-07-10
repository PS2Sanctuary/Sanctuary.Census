using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a service that contributes data to given types.
/// </summary>
public interface IContributionService
{
    /// <summary>
    /// Builds a list of records of the given type.
    /// </summary>
    /// <typeparam name="T">The type to build.</typeparam>
    /// <param name="typeCreationFactory">
    /// A factory that creates a initial instance of the type with the given ID.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A list of records of this type.</returns>
    Task<IReadOnlyList<T>> BuildThroughContributions<T>
    (
        Func<uint, T> typeCreationFactory,
        CancellationToken ct = default
    ) where T : class;
}

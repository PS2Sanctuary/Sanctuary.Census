using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Abstractions.Database;

/// <summary>
/// Represents a database context of the collections.
/// </summary>
public interface ICollectionsContext
{
    /// <summary>
    /// Ensures that the database structure is prepared.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ScaffoldAsync(CancellationToken ct = default);

    /// <summary>
    /// Upserts a collection in the database.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="data">The collection data.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertCollectionAsync<T>(IEnumerable<T> data, CancellationToken ct = default)
        where T : ISanctuaryCollection;
}

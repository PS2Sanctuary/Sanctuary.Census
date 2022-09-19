using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System;
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
    /// Retrieves all the documents of a collection.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection to retrieve.</typeparam>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The collection's documents.</returns>
    IAsyncEnumerable<TCollection> GetCollectionDocumentsAsync<TCollection>(CancellationToken ct = default)
        where TCollection : ISanctuaryCollection;

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
    /// <param name="additionalRemoveOldEntryTest">
    /// A delegate that returns a boolean value indicating whether the given <typeparamref name="T"/>
    /// object should be removed from the database, if it is not present in the updated data source.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        CancellationToken ct = default,
        Func<T, bool>? additionalRemoveOldEntryTest = null
    ) where T : ISanctuaryCollection;
}

using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Database;

/// <summary>
/// Represents a configured database collection.
/// </summary>
public interface ICollectionDbConfiguration
{
    /// <summary>
    /// Scaffolds the collection within the database.
    /// </summary>
    /// <param name="database">The database to scaffold the collection within.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ScaffoldAsync(IMongoContext database, CancellationToken ct = default);
}

/// <summary>
/// Provides means to configure an <see cref="ISanctuaryCollection"/>.
/// </summary>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
public class CollectionDbConfiguration<TCollection> : ICollectionDbConfiguration
    where TCollection : ISanctuaryCollection
{
    private readonly List<Expression<Func<TCollection, object?>>> _equalitySelectors;
    private readonly List<(Expression<Func<TCollection, object?>>, bool)> _indexes;

    /// <summary>
    /// Gets a list of selectors to retrieve the value of properties
    /// that the collection should be compared by.
    /// </summary>
    public IReadOnlyList<Expression<Func<TCollection, object?>>> EqualitySelectors => _equalitySelectors;

    /// <summary>
    /// Gets a list of selectors used to retrieve the value of properties
    /// that should be indexed on the collection.
    /// </summary>
    public IReadOnlyList<(Expression<Func<TCollection, object?>> Selector, bool IsUnique)> IndexPropertySelectors => _indexes;

    /// <summary>
    /// Stores a delegate that returns a boolean value indicating
    /// whether the given <typeparamref name="TCollection"/>
    /// object should be removed from the database, if it is
    /// not present in the updated data source.
    /// </summary>
    public Func<TCollection, bool> RemoveOldEntryTest { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this collection stores dynamic data.
    /// </summary>
    public bool IsDynamicCollection { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDbConfiguration{TCollection}"/> class.
    /// </summary>
    public CollectionDbConfiguration()
    {
        _equalitySelectors = new List<Expression<Func<TCollection, object?>>>();
        _indexes = new List<(Expression<Func<TCollection, object?>>, bool)>();
        RemoveOldEntryTest = static _ => true;
    }

    /// <summary>
    /// Indicates the given property should be indexed.
    /// </summary>
    /// <param name="indexPropertySelector">The property to index.</param>
    /// <param name="unique">Indicates whether the index should be unique.</param>
    /// <returns>The <see cref="CollectionDbConfiguration{TCollection}"/> instance, so that calls may be chained.</returns>
    public CollectionDbConfiguration<TCollection> WithIndex
    (
        Expression<Func<TCollection, object?>> indexPropertySelector,
        bool unique
    )
    {
        _indexes.Add((indexPropertySelector, unique));
        return this;
    }

    /// <summary>
    /// Indicates that the given property should act as an equality key
    /// when comparing records for upserting.
    /// </summary>
    /// <param name="keyPropertySelector">The property to use as an equality key.</param>
    /// <returns>The <see cref="CollectionDbConfiguration{TCollection}"/> instance, so that calls may be chained.</returns>
    public CollectionDbConfiguration<TCollection> WithEqualityKey
    (
        Expression<Func<TCollection, object?>> keyPropertySelector
    )
    {
        _equalitySelectors.Add(keyPropertySelector);
        return this;
    }

    /// <summary>
    /// Provide a delegate that returns a boolean value indicating
    /// whether the given <typeparamref name="TCollection"/>
    /// object should be removed from the database, if it is
    /// not present in the updated data source.
    /// </summary>
    /// <param name="test">The test delegate.</param>
    public CollectionDbConfiguration<TCollection> WithRemoveOldEntryTest(Func<TCollection, bool> test)
    {
        RemoveOldEntryTest = test;
        return this;
    }

    /// <summary>
    /// Indicates that the given collection contains dynamic data.
    /// </summary>
    /// <returns>The <see cref="CollectionDbConfiguration{TCollection}"/> instance, so that calls may be chained.</returns>
    public CollectionDbConfiguration<TCollection> IsDynamic()
    {
        IsDynamicCollection = true;
        return this;
    }

    /// <inheritdoc />
    public async Task ScaffoldAsync(IMongoContext database, CancellationToken ct = default)
    {
        if (IndexPropertySelectors.Count == 0)
            return;

        IMongoCollection<TCollection> collection = database.GetCollection<TCollection>();
        await collection.Indexes
            .CreateManyAsync
            (
                IndexPropertySelectors.Select(x => new CreateIndexModel<TCollection>
                (
                    Builders<TCollection>.IndexKeys.Ascending(x.Selector),
                    new CreateIndexOptions { Unique = x.IsUnique }
                )),
                ct
            )
            .ConfigureAwait(false);
    }
}

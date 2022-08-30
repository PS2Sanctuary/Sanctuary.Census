using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sanctuary.Census.Builder.Abstractions.Database;

/// <summary>
/// Provides the ability to scaffold and upsert a database collection.
/// </summary>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
public interface ICollectionDbConfiguration<TCollection> where TCollection : ISanctuaryCollection
{
    /// <summary>
    /// Gets a list of selectors to retrieve the value of properties
    /// that the collection should be compared by.
    /// </summary>
    IReadOnlyList<Expression<Func<TCollection, object?>>> EqualitySelectors { get; }

    /// <summary>
    /// Gets a list of selectors used to retrieve the value of properties
    /// that should be indexed on the collection.
    /// </summary>
    IReadOnlyList<(Expression<Func<TCollection, object?>> Selector, bool IsUnique)> IndexPropertySelectors { get; }

    /// <summary>
    /// Gets a value indicating whether old entries should be
    /// removed from the database when upserting.
    /// </summary>
    bool RemoveOldEntries { get; }

    /// <summary>
    /// Indicates the given property should be indexed.
    /// </summary>
    /// <param name="indexPropertySelector">The property to index.</param>
    /// <param name="unique">Indicates whether the index should be unique.</param>
    /// <returns>The <see cref="ICollectionDbConfiguration{TCollection}"/> instance, so that calls may be chained.</returns>
    ICollectionDbConfiguration<TCollection> WithIndex
    (
        Expression<Func<TCollection, object?>> indexPropertySelector,
        bool unique
    );

    /// <summary>
    /// Indicates that the given property should act as an equality key
    /// when comparing records for upserting.
    /// </summary>
    /// <param name="keyPropertySelector">The property to use as an equality key.</param>
    /// <returns>The <see cref="ICollectionDbConfiguration{TCollection}"/> instance, so that calls may be chained.</returns>
    ICollectionDbConfiguration<TCollection> WithEqualityKey
    (
        Expression<Func<TCollection, object?>> keyPropertySelector
    );

    /// <summary>
    /// Indicates whether old entries should be removed from the database.
    /// </summary>
    /// <param name="value"><c>True</c> to remove old entries, otherwise <c>false</c>.</param>
    ICollectionDbConfiguration<TCollection> WithRemoveOld(bool value = true);
}

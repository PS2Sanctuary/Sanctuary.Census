using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sanctuary.Census.Builder.Database;

/// <summary>
/// Provides means to configure an <see cref="ISanctuaryCollection"/>.
/// </summary>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
public class CollectionDbConfiguration<TCollection> : ICollectionDbConfiguration<TCollection>
    where TCollection : ISanctuaryCollection
{
    private readonly List<Expression<Func<TCollection, object?>>> _equalitySelectors;
    private readonly List<(Expression<Func<TCollection, object?>>, bool)> _indexes;

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<TCollection, object?>>> EqualitySelectors => _equalitySelectors;

    /// <inheritdoc />
    public IReadOnlyList<(Expression<Func<TCollection, object?>> Selector, bool IsUnique)> IndexPropertySelectors => _indexes;

    /// <inheritdoc />
    public bool RemoveOldEntries { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDbConfiguration{TCollection}"/> class.
    /// </summary>
    public CollectionDbConfiguration()
    {
        _equalitySelectors = new List<Expression<Func<TCollection, object?>>>();
        _indexes = new List<(Expression<Func<TCollection, object?>>, bool)>();
    }

    /// <inheritdoc />
    public ICollectionDbConfiguration<TCollection> WithIndex
    (
        Expression<Func<TCollection, object?>> indexPropertySelector,
        bool unique
    )
    {
        _indexes.Add((indexPropertySelector, unique));
        return this;
    }

    /// <inheritdoc />
    public ICollectionDbConfiguration<TCollection> WithEqualityKey
    (
        Expression<Func<TCollection, object?>> keyPropertySelector
    )
    {
        _equalitySelectors.Add(keyPropertySelector);
        return this;
    }

    /// <inheritdoc />
    public ICollectionDbConfiguration<TCollection> WithRemoveOld(bool value = true)
    {
        RemoveOldEntries = value;
        return this;
    }
}

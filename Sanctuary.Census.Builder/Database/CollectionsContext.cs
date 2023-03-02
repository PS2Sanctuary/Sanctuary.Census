using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Database;

/// <inheritdoc />
public class CollectionsContext : ICollectionsContext
{
    private static readonly JsonNamingPolicy NameConverter = new SnakeCaseJsonNamingPolicy();

    private readonly IMongoContext _database;
    private readonly ICollectionDiffService _diffService;
    private readonly CollectionConfigurationProvider _collectionConfigProvider;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsContext"/> class.
    /// </summary>
    /// <param name="mongoContext">The MongoDB context.</param>
    /// <param name="diffService">The collection diff service.</param>
    /// <param name="collectionConfigProvider">The collection config provider.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public CollectionsContext
    (
        IMongoContext mongoContext,
        ICollectionDiffService diffService,
        CollectionConfigurationProvider collectionConfigProvider,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _database = mongoContext;
        _diffService = diffService;
        _collectionConfigProvider = collectionConfigProvider;
        _environmentContextProvider = environmentContextProvider;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TCollection> GetCollectionDocumentsAsync<TCollection>([EnumeratorCancellation] CancellationToken ct = default)
        where TCollection : ISanctuaryCollection
    {
        IAsyncCursor<TCollection> cursor = await _database.GetCollection<TCollection>()
            .FindAsync(FilterDefinition<TCollection>.Empty, cancellationToken: ct)
            .ConfigureAwait(false);

        while (await cursor.MoveNextAsync(ct).ConfigureAwait(false))
        {
            foreach (TCollection value in cursor.Current)
                yield return value;
        }
    }

    /// <inheritdoc />
    public async Task ScaffoldAsync(CancellationToken ct = default)
    {
        foreach (ICollectionDbConfiguration configuration in _collectionConfigProvider.GetAll().Values)
            await configuration.ScaffoldAsync(_database, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        CancellationToken ct = default,
        Func<T, bool>? additionalRemoveOldEntryTest = null
    ) where T : ISanctuaryCollection
    {
        CollectionDbConfiguration<T> configuration = _collectionConfigProvider.GetConfiguration<T>();
        Func<T, bool> removeOldEntryTest = additionalRemoveOldEntryTest is null
            ? configuration.RemoveOldEntryTest
            : v => configuration.RemoveOldEntryTest(v) && additionalRemoveOldEntryTest(v);

        await UpsertCollectionAsync
        (
            data,
            configuration.EqualitySelectors,
            removeOldEntryTest,
            !configuration.IsDynamicCollection,
            ct
        ).ConfigureAwait(false);
    }

    private async Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        IReadOnlyList<Expression<Func<T, object?>>> equalitySelectors,
        Func<T, bool> oldEntryRemovalTest,
        bool includeInDiff,
        CancellationToken ct = default
    ) where T : ISanctuaryCollection
    {
        if (equalitySelectors.Count == 0)
            throw new InvalidOperationException("A collection must have at least one equality key configured. Type: " + typeof(T));

        List<T> dataList = data as List<T> ?? data.ToList();
        List<Func<T, object?>> compiledSelectors = equalitySelectors.Select(x => x.Compile()).ToList();

        IMongoCollection<T> collection = _database.GetCollection<T>();
        List<WriteModel<T>> dbWriteModels = new();

        bool isNewCollection = true;

        // Iterate over each document that's currently in the collection
        IAsyncCursor<T> cursor = await collection.FindAsync(new BsonDocument(), cancellationToken: ct).ConfigureAwait(false);
        while (await cursor.MoveNextAsync(ct).ConfigureAwait(false))
        {
            foreach (T document in cursor.Current)
            {
                isNewCollection = false;

                // Attempt to find the DB document in our upsert data
                int itemIndex = -1;
                for (int i = 0; i < dataList.Count; i++)
                {
                    bool found = compiledSelectors.All
                    (
                        comparator => comparator(document)?.Equals(comparator(dataList[i])) is true
                    );

                    if (!found)
                        continue;

                    itemIndex = i;
                    break;
                }

                if (itemIndex == -1)
                {
                    // We don't have the document in our upsert data, so it must have been deleted
                    if (oldEntryRemovalTest(document))
                    {
                        FilterDefinition<T> filter = BuildFilter(equalitySelectors, compiledSelectors, document);
                        DeleteOneModel<T> deleteModel = new(filter);

                        dbWriteModels.Add(deleteModel);
                        if (includeInDiff)
                            _diffService.SetDeleted(document);
                    }
                }
                else if (!dataList[itemIndex].Equals(document))
                {
                    // The documents don't match, so there's been a change
                    T item = dataList[itemIndex];

                    FilterDefinition<T> filter = BuildFilter(equalitySelectors, compiledSelectors, item);
                    ReplaceOneModel<T> upsertModel = new(filter, item);

                    dbWriteModels.Add(upsertModel);
                    if (includeInDiff)
                        _diffService.SetUpdated(document, item);
                }

                // No need to worry about the document any more
                // Remove it to improve list search performance
                if (itemIndex > -1)
                    dataList.RemoveAt(itemIndex);
            }
        }

        if (isNewCollection && dataList.Count > 0 && includeInDiff)
        {
            string collName = NameConverter.ConvertName(typeof(T).Name);
            _diffService.SetAdded(new NewCollection
            (
                collName,
                $"/get/{_environmentContextProvider.Environment}/{collName}"
            ));
        }

        // We've previously removed any deleted or updated documents
        // so what's remaining must be new
        foreach (T item in dataList)
        {
            InsertOneModel<T> insertModel = new(item);
            dbWriteModels.Add(insertModel);

            if (!isNewCollection && includeInDiff)
                _diffService.SetAdded(item);
        }

        if (dbWriteModels.Count > 0)
            await collection.BulkWriteAsync(dbWriteModels, null, ct).ConfigureAwait(false);
    }

    private static FilterDefinition<T> BuildFilter<T>
    (
        IReadOnlyList<Expression<Func<T, object?>>> equalitySelectors,
        IReadOnlyList<Func<T, object?>> compiledSelectors,
        T searchItem
    )
    {
        if (equalitySelectors.Count == 0)
            throw new InvalidOperationException("Must provide at least one selector");

        if (equalitySelectors.Count != compiledSelectors.Count)
            throw new InvalidOperationException("Must provide the same number of compiled selectors as expressions");

        if (equalitySelectors.Count == 1)
            return Builders<T>.Filter.Eq(equalitySelectors[0], compiledSelectors[0](searchItem));

        FilterDefinition<T> filter = Builders<T>.Filter.Empty;
        for (int i = 0; i < equalitySelectors.Count; i++)
            filter &= Builders<T>.Filter.Eq(equalitySelectors[i], compiledSelectors[i](searchItem));
        return filter;
    }
}

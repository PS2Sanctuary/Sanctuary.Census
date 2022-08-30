using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Extensions;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public async Task ScaffoldAsync(CancellationToken ct = default)
    {
        foreach (ICollectionDbConfiguration<ISanctuaryCollection> configuration in _collectionConfigProvider.GetAll().Values)
            await configuration.ScaffoldAsync(_database, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpsertCollectionAsync<T>(IEnumerable<T> data, CancellationToken ct = default)
        where T : ISanctuaryCollection
    {
        ICollectionDbConfiguration<T> configuration = _collectionConfigProvider.GetConfiguration<T>();
        await UpsertCollectionAsync
        (
            data,
            configuration.EqualitySelectors,
            configuration.RemoveOldEntries,
            ct
        ).ConfigureAwait(false);
    }

    private async Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        IReadOnlyList<Expression<Func<T, object?>>> equalitySelectors,
        bool removeOld = true,
        CancellationToken ct = default
    ) where T : ISanctuaryCollection
    {
        if (equalitySelectors.Count == 0)
            throw new InvalidOperationException("A collection must have at least one equality key configured. Type: " + typeof(T));

        List<T> dataList = data as List<T> ?? data.ToList();
        List<Func<T, object?>> compiledComparators = equalitySelectors.Select(x => x.Compile()).ToList();

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
                    bool found = compiledComparators.All
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
                    if (removeOld)
                    {
                        FilterDefinition<T> filter = Builders<T>.Filter.Empty;
                        for (int i = 0; i < equalitySelectors.Count; i++)
                            filter &= Builders<T>.Filter.Eq(equalitySelectors[i], compiledComparators[i](document));

                        // We don't have the document in our upsert data, so it must have been deleted
                        DeleteOneModel<T> deleteModel = new(filter);
                        dbWriteModels.Add(deleteModel);
                        _diffService.SetDeleted(document);
                    }
                }
                else if (!dataList[itemIndex].Equals(document))
                {
                    // The documents don't match, so there's been a change
                    T item = dataList[itemIndex];

                    FilterDefinition<T> filter = Builders<T>.Filter.Empty;
                    for (int i = 0; i < equalitySelectors.Count; i++)
                        filter &= Builders<T>.Filter.Eq(equalitySelectors[i], compiledComparators[i](item));

                    ReplaceOneModel<T> upsertModel = new(filter, item);
                    dbWriteModels.Add(upsertModel);
                    _diffService.SetUpdated(document, item);
                }

                // No need to worry about the document any more
                if (itemIndex > -1)
                    dataList.RemoveAt(itemIndex);
            }
        }

        if (isNewCollection && dataList.Count > 0)
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

            if (!isNewCollection)
                _diffService.SetAdded(item);
        }

        if (dbWriteModels.Count > 0)
            await collection.BulkWriteAsync(dbWriteModels, null, ct).ConfigureAwait(false);
    }
}

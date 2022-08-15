using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Database;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Services;

/// <inheritdoc />
public class CollectionDiffService : ICollectionDiffService
{
    private readonly IMongoContext _mongoContext;
    private readonly List<CollectionDiffEntry> _entries;
    private readonly Dictionary<Type, string> _collectionNames;
    private readonly Dictionary<string, uint> _collectionChangeCounts;

    private DateTime _diffStarted;

    static CollectionDiffService()
    {
        BsonClassMap.RegisterClassMap<CollectionDiffEntry>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<DiffRecord>(MongoContext.AutoClassMap);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDiffService"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB context.</param>
    public CollectionDiffService(IMongoContext mongoContext)
    {
        _mongoContext = mongoContext;
        _entries = new List<CollectionDiffEntry>();
        _collectionNames = new Dictionary<Type, string>();
        _collectionChangeCounts = new Dictionary<string, uint>();

        _diffStarted = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public void SetAdded<T>(T document)
    {
        CollectionDiffEntry entry = new
        (
            _diffStarted,
            GetCollectionName<T>(),
            null,
            document
        );
        _entries.Add(entry);
        IncrementCollectionChanges<T>();
    }

    /// <inheritdoc />
    public void SetDeleted<T>(T document)
    {
        CollectionDiffEntry entry = new
        (
            _diffStarted,
            GetCollectionName<T>(),
            document,
            null
        );
        _entries.Add(entry);
        IncrementCollectionChanges<T>();
    }

    /// <inheritdoc />
    public void SetUpdated<T>(T oldDocument, T newDocument)
    {
        CollectionDiffEntry entry = new
        (
            _diffStarted,
            GetCollectionName<T>(),
            oldDocument,
            newDocument
        );
        _entries.Add(entry);
        IncrementCollectionChanges<T>();
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_entries.Count == 0)
        {
            _diffStarted = DateTime.UtcNow;
            return;
        }

        IMongoCollection<CollectionDiffEntry> diffEntryColl = _mongoContext.GetCollection<CollectionDiffEntry>();
        IMongoCollection<DiffRecord> diffRecordColl = _mongoContext.GetCollection<DiffRecord>();

        await diffEntryColl.Indexes.CreateManyAsync
        (
            new[] {
                new CreateIndexModel<CollectionDiffEntry>(Builders<CollectionDiffEntry>.IndexKeys.Descending(x => x.DiffTime)),
                new CreateIndexModel<CollectionDiffEntry>(Builders<CollectionDiffEntry>.IndexKeys.Ascending(x => x.CollectionName))
            },
            ct
        ).ConfigureAwait(false);

        await diffRecordColl.Indexes.CreateOneAsync
        (
            new CreateIndexModel<DiffRecord>(Builders<DiffRecord>.IndexKeys.Descending
            (
                x => x.GeneratedAt),
                new CreateIndexOptions { Unique = true }
            ),
            cancellationToken: ct
        ).ConfigureAwait(false);

        await diffEntryColl.InsertManyAsync(_entries, cancellationToken: ct).ConfigureAwait(false);
        _entries.Clear();

        DiffRecord record = new(_diffStarted, _collectionChangeCounts);
        await diffRecordColl.InsertOneAsync(record, cancellationToken: ct).ConfigureAwait(false);

        _diffStarted = DateTime.UtcNow;
    }

    private string GetCollectionName<T>()
    {
        if (_collectionNames.TryGetValue(typeof(T), out string? name))
            return name;

        _collectionNames[typeof(T)] = SnakeCaseJsonNamingPolicy.Default.ConvertName(typeof(T).Name);
        return _collectionNames[typeof(T)];
    }

    private void IncrementCollectionChanges<T>()
    {
        string name = GetCollectionName<T>();
        if (!_collectionChangeCounts.ContainsKey(name))
            _collectionChangeCounts[name] = 0;

        _collectionChangeCounts[name]++;
    }
}

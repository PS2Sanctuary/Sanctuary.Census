using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Database;

/// <inheritdoc />
public class MongoContext : IMongoContext
{
    /// <summary>
    /// Gets the name converter, used to translate member names of the POCO
    /// classes supported by this context to their database representation.
    /// </summary>
    public static readonly JsonNamingPolicy NameConverter = new SnakeCaseJsonNamingPolicy();

    private readonly MongoClient _mongoClient;
    private readonly EnvironmentContextProvider _environmentContextProvider;
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoContext"/> class.
    /// </summary>
    /// <param name="mongoClient">The mongo client to use.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public MongoContext
    (
        MongoClient mongoClient,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _mongoClient = mongoClient;
        _environmentContextProvider = environmentContextProvider;
        _database = GetDatabase();
    }

    /// <inheritdoc />
    public IMongoDatabase GetDatabase()
        => _mongoClient.GetDatabase(_environmentContextProvider.Environment + "-collections");

    /// <inheritdoc />
    public async Task ScaffoldAsync(CancellationToken ct = default)
    {
        await CreateUniqueKeyIndex<Currency>(x => x.CurrencyID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Experience>(x => x.ExperienceID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Faction>(x => x.FactionID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<FireGroup>(x => x.FireGroupID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexs<FireGroupToFireMode>(ct, x => x.FireGroupId, x => x.FireModeId);
        await CreateUniqueKeyIndex<FireMode2>(x => x.FireModeID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexs<FireModeToProjectile>(ct, x => x.FireModeID, x => x.ProjectileID);
        await CreateUniqueKeyIndex<Item>(x => x.ItemID, ct).ConfigureAwait(false);
        // TODO: Test
        await CreateNonUniqueKeyIndexs<Item>
        (
            ct,
            x => x.Name.Chinese,
            x => x.Name.English,
            x => x.Name.French,
            x => x.Name.German,
            x => x.Name.Italian,
            x => x.Name.Korean,
            x => x.Name.Portuguese,
            x => x.Name.Russian,
            x => x.Name.Spanish,
            x => x.Name.Turkish
        );
        await CreateUniqueKeyIndex<ItemCategory>(x => x.ItemCategoryID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexs<ItemToWeapon>(ct, x => x.ItemId, x => x.WeaponId).ConfigureAwait(false);
        await CreateUniqueKeyIndex<PlayerStateGroup2>(x => x.PlayerStateGroupId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Profile>(x => x.ProfileId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Projectile>(x => x.ProjectileId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Weapon>(x => x.WeaponId, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexs<WeaponAmmoSlot>(ct, x => x.WeaponId).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexs<WeaponToFireGroup>(ct, x => x.WeaponId, x => x.FireGroupId).ConfigureAwait(false);
        await CreateUniqueKeyIndex<World>(x => x.WorldID, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        Func<T, FilterDefinition<T>> elementFilter,
        CancellationToken ct
    )
    {
        IMongoCollection<T> collection = GetCollection<T>();

        List<WriteModel<T>> upserts = new();
        foreach (T element in data)
        {
            ReplaceOneModel<T> upsertModel = new(elementFilter(element), element)
            {
                IsUpsert = true
            };
            upserts.Add(upsertModel);
        }

        await collection.BulkWriteAsync(upserts, null, ct);
    }

    /// <inheritdoc />
    public async Task UpsertCurrenciesAsync(IEnumerable<Currency> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Currency>.Filter.Eq(x => x.CurrencyID, e.CurrencyID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertExperiencesAsync(IEnumerable<Experience> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Experience>.Filter.Eq(x => x.ExperienceID, e.ExperienceID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFactionsAsync(IEnumerable<Faction> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Faction>.Filter.Eq(x => x.FactionID, e.FactionID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireGroupsAsync(IEnumerable<FireGroup> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<FireGroup>.Filter.Eq(x => x.FireGroupID, e.FireGroupID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireGroupsToFireModesAsync(IEnumerable<FireGroupToFireMode> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<FireGroupToFireMode>.Filter.Where(x => x.FireGroupId == e.FireGroupId && x.FireModeId == e.FireModeId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireMode2sAsync(IEnumerable<FireMode2> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<FireMode2>.Filter.Eq(x => x.FireModeID, e.FireModeID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireModesToProjectilesAsync(IEnumerable<FireModeToProjectile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<FireModeToProjectile>.Filter.Where(x => x.FireModeID == e.FireModeID && x.ProjectileID == e.ProjectileID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemsAsync(IEnumerable<Item> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Item>.Filter.Eq(x => x.ItemID, e.ItemID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemCategorysAsync(IEnumerable<ItemCategory> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<ItemCategory>.Filter.Eq(x => x.ItemCategoryID, e.ItemCategoryID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemsToWeaponsAsync(IEnumerable<ItemToWeapon> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<ItemToWeapon>.Filter.Where(x => x.ItemId == e.ItemId && x.WeaponId == e.WeaponId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertPlayerStateGroup2Async(IEnumerable<PlayerStateGroup2> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<PlayerStateGroup2>.Filter.Eq(x => x.PlayerStateGroupId, e.PlayerStateGroupId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertProfilesAsync(IEnumerable<Profile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Profile>.Filter.Eq(x => x.ProfileId, e.ProfileId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertProjectilesAsync(IEnumerable<Projectile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Projectile>.Filter.Eq(x => x.ProjectileId, e.ProjectileId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponsAsync(IEnumerable<Weapon> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<Weapon>.Filter.Eq(x => x.WeaponId, e.WeaponId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponAmmoSlotsAsync(IEnumerable<WeaponAmmoSlot> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<WeaponAmmoSlot>.Filter.Where(x => x.WeaponId == e.WeaponId && x.WeaponSlotIndex == e.WeaponSlotIndex),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponsToFireGroupsAsync(IEnumerable<WeaponToFireGroup> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<WeaponToFireGroup>.Filter.Where(x => x.WeaponId == e.WeaponId && x.FireGroupId == e.FireGroupId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWorldsAsync(IEnumerable<World> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => Builders<World>.Filter.Eq(x => x.WorldID, e.WorldID),
            ct
        ).ConfigureAwait(false);

    private IMongoCollection<T> GetCollection<T>()
        => _database.GetCollection<T>(NameConverter.ConvertName(typeof(T).Name));

    private async Task CreateUniqueKeyIndex<T>(Expression<Func<T, object>> indexPropertySelector, CancellationToken ct)
    {
        IMongoCollection<T> collection = GetCollection<T>();
        await collection.Indexes.CreateOneAsync
        (
            new CreateIndexModel<T>
            (
                Builders<T>.IndexKeys.Ascending(indexPropertySelector),
                new CreateIndexOptions { Unique = true }
            ),
            null,
            ct
        ).ConfigureAwait(false);
    }

    private async Task CreateNonUniqueKeyIndexs<T>(CancellationToken ct, params Expression<Func<T, object>>[] indexPropertySelector)
    {
        IMongoCollection<T> collection = GetCollection<T>();

        if (indexPropertySelector.Length == 1)
        {
            await collection.Indexes.CreateOneAsync
            (
                new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(indexPropertySelector[0])),
                null,
                ct
            ).ConfigureAwait(false);
        }
        else
        {
            await collection.Indexes.CreateManyAsync
            (
                indexPropertySelector.Select(s => new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(s))),
                ct
            ).ConfigureAwait(false);
        }
    }

    static MongoContext()
    {
        BsonClassMap.RegisterClassMap<Currency>(AutoMap);
        BsonClassMap.RegisterClassMap<Experience>(AutoMap);
        BsonClassMap.RegisterClassMap<Faction>(AutoMap);
        BsonClassMap.RegisterClassMap<FireGroup>(AutoMap);
        BsonClassMap.RegisterClassMap<FireGroupToFireMode>(AutoMap);
        BsonClassMap.RegisterClassMap<FireMode2>(AutoMap);
        BsonClassMap.RegisterClassMap<FireModeToProjectile>(AutoMap);
        BsonClassMap.RegisterClassMap<Item>(AutoMap);
        BsonClassMap.RegisterClassMap<ItemCategory>(AutoMap);
        BsonClassMap.RegisterClassMap<ItemToWeapon>(AutoMap);
        BsonClassMap.RegisterClassMap<PlayerStateGroup2>(AutoMap);
        BsonClassMap.RegisterClassMap<Profile>(AutoMap);
        BsonClassMap.RegisterClassMap<Projectile>(AutoMap);
        BsonClassMap.RegisterClassMap<Weapon>(AutoMap);
        BsonClassMap.RegisterClassMap<WeaponAmmoSlot>(AutoMap);
        BsonClassMap.RegisterClassMap<WeaponToFireGroup>(AutoMap);
        BsonClassMap.RegisterClassMap<World>(AutoMap);

        BsonClassMap.RegisterClassMap<LocaleString>(cm =>
        {
            cm.UnmapProperty(x => x.ID);
            cm.MapProperty(x => x.German).SetElementName("de");
            cm.MapProperty(x => x.English).SetElementName("en");
            cm.MapProperty(x => x.Spanish).SetElementName("es");
            cm.MapProperty(x => x.French).SetElementName("fr");
            cm.MapProperty(x => x.Italian).SetElementName("it");
            cm.MapProperty(x => x.Korean).SetElementName("ko");
            cm.MapProperty(x => x.Portuguese).SetElementName("pt");
            cm.MapProperty(x => x.Russian).SetElementName("ru");
            cm.MapProperty(x => x.Turkish).SetElementName("tr");
            cm.MapProperty(x => x.Chinese).SetElementName("zh");
        });
    }

    private static void AutoMap<T>(BsonClassMap<T> cm)
    {
        foreach (PropertyInfo prop in typeof(T).GetProperties())
            cm.MapProperty(prop.Name).SetElementName(NameConverter.ConvertName(prop.Name));
    }
}

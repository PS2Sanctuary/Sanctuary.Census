using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using Sanctuary.Census.Models.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Database;

/// <inheritdoc />
public class CollectionsContext : ICollectionsContext
{
    private static readonly JsonNamingPolicy NameConverter = new SnakeCaseJsonNamingPolicy();

    private readonly IMongoDatabase _database;
    private readonly ICollectionDiffService _diffService;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsContext"/> class.
    /// </summary>
    /// <param name="mongoContext">The MongoDB context.</param>
    /// <param name="diffService">The collection diff service.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public CollectionsContext
    (
        IMongoContext mongoContext,
        ICollectionDiffService diffService,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _database = mongoContext.GetDatabase();
        _diffService = diffService;
        _environmentContextProvider = environmentContextProvider;
    }

    /// <inheritdoc />
    public async Task ScaffoldAsync(CancellationToken ct = default)
    {
        await CreateUniqueKeyIndex<Currency>(x => x.CurrencyID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Experience>(x => x.ExperienceID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<FacilityInfo>(ct, x => x.ZoneID, x => x.FacilityID).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<FacilityLink>(ct, x => x.ZoneID, x => x.FacilityIdA, x => x.FacilityIdB).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Faction>(x => x.FactionID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<FireGroup>(x => x.FireGroupID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<FireGroupToFireMode>(ct, x => x.FireGroupId, x => x.FireModeId);
        await CreateUniqueKeyIndex<FireMode2>(x => x.FireModeID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<FireModeToProjectile>(ct, x => x.FireModeID, x => x.ProjectileID);
        await CreateUniqueKeyIndex<Item>(x => x.ItemID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<Item>
        (
            ct,
            x => x.Name!.Zh!,
            x => x.Name!.En!,
            x => x.Name!.Fr!,
            x => x.Name!.De!,
            x => x.Name!.It!,
            x => x.Name!.Ko!,
            x => x.Name!.Pt!,
            x => x.Name!.Ru!,
            x => x.Name!.Es!,
            x => x.Name!.Tr!
        );
        await CreateUniqueKeyIndex<ItemCategory>(x => x.ItemCategoryID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<ItemToWeapon>(ct, x => x.ItemId, x => x.WeaponId).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Loadout>(x => x.LoadoutID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<LoadoutSlot>(ct, x => x.LoadoutID, x => x.SlotID).ConfigureAwait(false);
        await CreateUniqueKeyIndex<MapRegion>(x => x.MapRegionId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<OutfitWarRegistration>(x => x.OutfitID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<PlayerStateGroup2>(ct, x => x.PlayerStateGroupId, x => x.PlayerStateId).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Profile>(x => x.ProfileId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Projectile>(x => x.ProjectileId, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Vehicle>(x => x.VehicleId, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<VehicleAttachment>(ct, x => x.ItemID, x => x.VehicleLoadoutID, x => x.VehicleID).ConfigureAwait(false);
        await CreateUniqueKeyIndex<VehicleLoadout>(x => x.LoadoutID, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<VehicleLoadoutSlot>(ct, x => x.LoadoutID, x => x.SlotID).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<VehicleSkillSet>(ct, x => x.VehicleID, x => x.SkillSetID, x => x.FactionID).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Weapon>(x => x.WeaponId, ct).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<WeaponAmmoSlot>(ct, x => x.WeaponId).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<WeaponToAttachment>(ct, x => x.AttachmentID, x => x.ItemID).ConfigureAwait(false);
        await CreateNonUniqueKeyIndexes<WeaponToFireGroup>(ct, x => x.WeaponId, x => x.FireGroupId).ConfigureAwait(false);
        await CreateUniqueKeyIndex<World>(x => x.WorldID, ct).ConfigureAwait(false);
        await CreateUniqueKeyIndex<Models.Collections.Zone>(x => x.ZoneID, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> data,
        Func<T, Predicate<T>> comparator,
        Func<T, FilterDefinition<T>> elementFilter,
        CancellationToken ct,
        bool removeOld = true
    ) where T : class
    {
        List<T> dataList = data as List<T> ?? data.ToList();

        IMongoCollection<T> collection = GetCollection<T>();
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
                int itemIndex = dataList.FindIndex(comparator(document));

                if (itemIndex == -1)
                {
                    if (removeOld)
                    {
                        // We don't have the document in our upsert data, so it must have been deleted
                        DeleteOneModel<T> deleteModel = new(elementFilter(document));
                        dbWriteModels.Add(deleteModel);
                        _diffService.SetDeleted(document);
                    }
                }
                else if (!dataList[itemIndex].Equals(document))
                {
                    // The documents don't match, so there's been a change
                    T item = dataList[itemIndex];

                    ReplaceOneModel<T> upsertModel = new(elementFilter(item), item);
                    dbWriteModels.Add(upsertModel);
                    _diffService.SetUpdated(document, item);
                }

                // No need to worry about the document any more
                if (itemIndex > -1)
                    dataList.RemoveAt(itemIndex);
            }
        }

        if (isNewCollection)
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
            await collection.BulkWriteAsync(dbWriteModels, null, ct);
    }

    /// <inheritdoc />
    public async Task UpsertCurrenciesAsync(IEnumerable<Currency> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.CurrencyID == e.CurrencyID,
            e => Builders<Currency>.Filter.Eq(x => x.CurrencyID, e.CurrencyID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertExperiencesAsync(IEnumerable<Experience> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ExperienceID == e.ExperienceID,
            e => Builders<Experience>.Filter.Eq(x => x.ExperienceID, e.ExperienceID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFacilityInfosAsync(IEnumerable<FacilityInfo> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FacilityID == e.FacilityID,
            e => Builders<FacilityInfo>.Filter.Where(x => x.FacilityID == e.FacilityID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFacilityLinksAsync(IEnumerable<FacilityLink> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FacilityIdA == e.FacilityIdA && x.FacilityIdB == e.FacilityIdB,
            e => Builders<FacilityLink>.Filter.Where(x => x.FacilityIdA == e.FacilityIdA && x.FacilityIdB == e.FacilityIdB),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFactionsAsync(IEnumerable<Faction> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FactionID == e.FactionID,
            e => Builders<Faction>.Filter.Eq(x => x.FactionID, e.FactionID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireGroupsAsync(IEnumerable<FireGroup> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FireGroupID == e.FireGroupID,
            e => Builders<FireGroup>.Filter.Eq(x => x.FireGroupID, e.FireGroupID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireGroupsToFireModesAsync(IEnumerable<FireGroupToFireMode> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FireGroupId == e.FireGroupId && x.FireModeId == e.FireModeId && x.FireModeIndex == e.FireModeIndex,
            e => Builders<FireGroupToFireMode>.Filter.Where(x => x.FireGroupId == e.FireGroupId && x.FireModeId == e.FireModeId && x.FireModeIndex == e.FireModeIndex),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireMode2sAsync(IEnumerable<FireMode2> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FireModeID == e.FireModeID,
            e => Builders<FireMode2>.Filter.Eq(x => x.FireModeID, e.FireModeID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertFireModesToProjectilesAsync(IEnumerable<FireModeToProjectile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.FireModeID == e.FireModeID && x.ProjectileID == e.ProjectileID,
            e => Builders<FireModeToProjectile>.Filter.Where(x => x.FireModeID == e.FireModeID && x.ProjectileID == e.ProjectileID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemsAsync(IEnumerable<Item> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ItemID == e.ItemID,
            e => Builders<Item>.Filter.Eq(x => x.ItemID, e.ItemID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemCategorysAsync(IEnumerable<ItemCategory> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ItemCategoryID == e.ItemCategoryID,
            e => Builders<ItemCategory>.Filter.Eq(x => x.ItemCategoryID, e.ItemCategoryID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertItemsToWeaponsAsync(IEnumerable<ItemToWeapon> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ItemId == e.ItemId && x.WeaponId == e.WeaponId,
            e => Builders<ItemToWeapon>.Filter.Where(x => x.ItemId == e.ItemId && x.WeaponId == e.WeaponId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertLoadoutsAsync(IEnumerable<Loadout> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.LoadoutID == e.LoadoutID,
            e => Builders<Loadout>.Filter.Eq(x => x.LoadoutID, e.LoadoutID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertLoadoutSlotsAsync(IEnumerable<LoadoutSlot> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.LoadoutID == e.LoadoutID && x.SlotID == e.SlotID,
            e => Builders<LoadoutSlot>.Filter.Where(x => x.LoadoutID == e.LoadoutID && x.SlotID == e.SlotID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertMapRegionsAsync(IEnumerable<MapRegion> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.MapRegionId == e.MapRegionId,
            e => Builders<MapRegion>.Filter.Eq(x => x.MapRegionId, e.MapRegionId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertOutfitWarRegistrationsAsync(IEnumerable<OutfitWarRegistration> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.OutfitID == e.OutfitID,
            e => Builders<OutfitWarRegistration>.Filter.Eq(x => x.OutfitID, e.OutfitID),
            ct,
            false
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertPlayerStateGroup2Async(IEnumerable<PlayerStateGroup2> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.PlayerStateGroupId == e.PlayerStateGroupId && x.PlayerStateId == e.PlayerStateId,
            e => Builders<PlayerStateGroup2>.Filter.Where(x => x.PlayerStateGroupId == e.PlayerStateGroupId && x.PlayerStateId == e.PlayerStateId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertProfilesAsync(IEnumerable<Profile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ProfileId == e.ProfileId,
            e => Builders<Profile>.Filter.Eq(x => x.ProfileId, e.ProfileId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertProjectilesAsync(IEnumerable<Projectile> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ProjectileId == e.ProjectileId,
            e => Builders<Projectile>.Filter.Eq(x => x.ProjectileId, e.ProjectileId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertVehiclesAsync(IEnumerable<Vehicle> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.VehicleId == e.VehicleId,
            e => Builders<Vehicle>.Filter.Eq(x => x.VehicleId, e.VehicleId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertVehicleAttachmentsAsync(IEnumerable<VehicleAttachment> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ItemID == e.ItemID && x.VehicleLoadoutID == e.VehicleLoadoutID,
            e => Builders<VehicleAttachment>.Filter.Where(x => x.ItemID == e.ItemID && x.VehicleLoadoutID == e.VehicleLoadoutID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertVehicleLoadoutsAsync(IEnumerable<VehicleLoadout> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.LoadoutID == e.LoadoutID,
            e => Builders<VehicleLoadout>.Filter.Eq(x => x.LoadoutID, e.LoadoutID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertVehicleLoadoutSlotsAsync(IEnumerable<VehicleLoadoutSlot> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.LoadoutID == e.LoadoutID && x.SlotID == e.SlotID,
            e => Builders<VehicleLoadoutSlot>.Filter.Where(x => x.LoadoutID == e.LoadoutID && x.SlotID == e.SlotID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertVehicleSkillSetsAsync(IEnumerable<VehicleSkillSet> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.VehicleID == e.VehicleID && x.SkillSetID == e.SkillSetID && x.FactionID == e.FactionID,
            e => Builders<VehicleSkillSet>.Filter.Where(x => x.VehicleID == e.VehicleID && x.SkillSetID == e.SkillSetID && x.FactionID == e.FactionID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponsAsync(IEnumerable<Weapon> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.WeaponId == e.WeaponId,
            e => Builders<Weapon>.Filter.Eq(x => x.WeaponId, e.WeaponId),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponAmmoSlotsAsync(IEnumerable<WeaponAmmoSlot> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.WeaponId == e.WeaponId && x.WeaponSlotIndex == e.WeaponSlotIndex,
            e => Builders<WeaponAmmoSlot>.Filter.Where(x => x.WeaponId == e.WeaponId && x.WeaponSlotIndex == e.WeaponSlotIndex),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponToAttachmentsAsync(IEnumerable<WeaponToAttachment> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.AttachmentID == e.AttachmentID && x.ItemID == e.ItemID,
            e => Builders<WeaponToAttachment>.Filter.Where(x => x.AttachmentID == e.AttachmentID && x.ItemID == e.ItemID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWeaponsToFireGroupsAsync(IEnumerable<WeaponToFireGroup> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.WeaponId == e.WeaponId && x.FireGroupId == e.FireGroupId && x.FireGroupIndex == e.FireGroupIndex,
            e => Builders<WeaponToFireGroup>.Filter.Where(x => x.WeaponId == e.WeaponId && x.FireGroupId == e.FireGroupId && x.FireGroupIndex == e.FireGroupIndex),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertWorldsAsync(IEnumerable<World> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.WorldID == e.WorldID,
            e => Builders<World>.Filter.Eq(x => x.WorldID, e.WorldID),
            ct
        ).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task UpsertZonesAsync(IEnumerable<Models.Collections.Zone> collection, CancellationToken ct = default)
        => await UpsertCollectionAsync
        (
            collection,
            e => x => x.ZoneID == e.ZoneID,
            e => Builders<Models.Collections.Zone>.Filter.Eq(x => x.ZoneID, e.ZoneID),
            ct,
            false
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

    private async Task CreateNonUniqueKeyIndexes<T>(CancellationToken ct, params Expression<Func<T, object>>[] indexPropertySelector)
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

    static CollectionsContext()
    {
        BsonClassMap.RegisterClassMap<Currency>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Experience>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FacilityInfo>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FacilityLink>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Faction>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireGroup>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireGroupToFireMode>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireMode2>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireModeToProjectile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Item>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<ItemCategory>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<ItemToWeapon>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Loadout>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<LoadoutSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<MapRegion>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<OutfitWarRegistration>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<PlayerStateGroup2>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Profile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Projectile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Vehicle>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleAttachment>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleLoadout>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleLoadoutSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleSkillSet>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Weapon>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponAmmoSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponToAttachment>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponToFireGroup>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<World>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Models.Collections.Zone>(MongoContext.AutoClassMap);

        BsonClassMap.RegisterClassMap<LocaleString>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<NewCollection>(MongoContext.AutoClassMap);
    }
}

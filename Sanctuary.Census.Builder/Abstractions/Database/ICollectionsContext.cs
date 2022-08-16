using MongoDB.Driver;
using Sanctuary.Census.Common.Objects.Collections;
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
    /// Ensures that the database structure is prepared.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ScaffoldAsync(CancellationToken ct = default);

    /// <summary>
    /// Upserts a collection in the database.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="comparator">A comparator to use between CLR objects.</param>
    /// <param name="matchFilter">The Mongo filter used to match elements.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <param name="removeOld"><c>True</c> to removed old entries in the collection.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> collection,
        Func<T, Predicate<T>> comparator,
        Func<T, FilterDefinition<T>> matchFilter,
        CancellationToken ct = default,
        bool removeOld = true
    ) where T : class;

    /// <summary>
    /// Upserts the <see cref="Currency"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertCurrenciesAsync(IEnumerable<Currency> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Experience"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertExperiencesAsync(IEnumerable<Experience> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FacilityInfo"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFacilityInfosAsync(IEnumerable<FacilityInfo> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FacilityLink"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFacilityLinksAsync(IEnumerable<FacilityLink> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Faction"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFactionsAsync(IEnumerable<Faction> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FireGroup"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFireGroupsAsync(IEnumerable<FireGroup> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FireGroupToFireMode"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFireGroupsToFireModesAsync(IEnumerable<FireGroupToFireMode> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FireMode2"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFireMode2sAsync(IEnumerable<FireMode2> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="FireModeToProjectile"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertFireModesToProjectilesAsync(IEnumerable<FireModeToProjectile> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="ImageSet"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertImageSetsAsync(IEnumerable<ImageSet> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Item"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertItemsAsync(IEnumerable<Item> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="ItemCategory"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertItemCategorysAsync(IEnumerable<ItemCategory> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="ItemToWeapon"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertItemsToWeaponsAsync(IEnumerable<ItemToWeapon> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Loadout"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertLoadoutsAsync(IEnumerable<Loadout> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="LoadoutSlot"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertLoadoutSlotsAsync(IEnumerable<LoadoutSlot> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="MapRegion"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertMapRegionsAsync(IEnumerable<MapRegion> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="OutfitWar"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertOutfitWarsAsync(IEnumerable<OutfitWar> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="OutfitWarRegistration"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertOutfitWarRegistrationsAsync(IEnumerable<OutfitWarRegistration> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="OutfitWarRounds"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertOutfitWarRoundsAsync(IEnumerable<OutfitWarRounds> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="PlayerStateGroup2"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertPlayerStateGroup2Async(IEnumerable<PlayerStateGroup2> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Profile"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertProfilesAsync(IEnumerable<Profile> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Projectile"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertProjectilesAsync(IEnumerable<Projectile> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Vehicle"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertVehiclesAsync(IEnumerable<Vehicle> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="VehicleAttachment"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertVehicleAttachmentsAsync(IEnumerable<VehicleAttachment> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="VehicleLoadout"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertVehicleLoadoutsAsync(IEnumerable<VehicleLoadout> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="VehicleLoadoutSlot"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertVehicleLoadoutSlotsAsync(IEnumerable<VehicleLoadoutSlot> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="VehicleSkillSet"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertVehicleSkillSetsAsync(IEnumerable<VehicleSkillSet> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Weapon"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertWeaponsAsync(IEnumerable<Weapon> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="WeaponAmmoSlot"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertWeaponAmmoSlotsAsync(IEnumerable<WeaponAmmoSlot> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="WeaponToAttachment"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertWeaponToAttachmentsAsync(IEnumerable<WeaponToAttachment> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="WeaponToFireGroup"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertWeaponsToFireGroupsAsync(IEnumerable<WeaponToFireGroup> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="World"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertWorldsAsync(IEnumerable<World> collection, CancellationToken ct = default);

    /// <summary>
    /// Upserts the <see cref="Zone"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertZonesAsync(IEnumerable<Common.Objects.Collections.Zone> collection, CancellationToken ct = default);
}

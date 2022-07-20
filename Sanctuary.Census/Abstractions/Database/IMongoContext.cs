using MongoDB.Driver;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Models.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Abstractions.Database;

/// <summary>
/// Represents a MongoDB context.
/// </summary>
public interface IMongoContext
{
    /// <summary>
    /// Gets a connection to the database.
    /// </summary>
    /// <param name="environment">The environment to retrieve the database from.</param>
    /// <returns>The database.</returns>
    IMongoDatabase GetDatabase(PS2Environment? environment = null);

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
    /// <param name="matchFilter">The filter used to match elements.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertCollectionAsync<T>
    (
        IEnumerable<T> collection,
        Func<T, FilterDefinition<T>> matchFilter,
        CancellationToken ct = default
    );

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
    /// Upserts the <see cref="MapRegion"/> collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpsertMapRegionsAsync(IEnumerable<MapRegion> collection, CancellationToken ct = default);

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
}

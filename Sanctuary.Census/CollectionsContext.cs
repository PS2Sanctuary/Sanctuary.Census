using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using Sanctuary.Census.Models.Collections;
using System.Collections.Generic;

namespace Sanctuary.Census;

/// <summary>
/// Maintains a store of the DTO collections.
/// </summary>
public class CollectionsContext
{
    /// <summary>
    /// Gets the collection information.
    /// </summary>
    public IReadOnlyList<CollectionInfo> CollectionInfos { get; set; }

    /// <summary>
    /// Gets the Currency collection, indexed by <see cref="Currency.CurrencyID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Currency> Currencies { get; set; }

    /// <summary>
    /// Gets the Experience collection, indexed by <see cref="Experience.ExperienceID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Experience> Experiences { get; set; }

    /// <summary>
    /// Gets the Faction collection, indexed by <see cref="Faction.FactionID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Faction> Factions { get; set; }

    /// <summary>
    /// Gets the FireGroup collection, indexed by <see cref="FireGroup.FireGroupID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, FireGroup> FireGroups { get; set; }

    /// <summary>
    /// Gets the FireMode collection, indexed by <see cref="FireMode.FireModeID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, FireMode> FireModes { get; set; }

    /// <summary>
    /// Gets the FireModeToProjectile collection, indexed by <see cref="FireModeToProjectile.FireModeID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, FireModeToProjectile> FireModeToProjectileMap { get; set; }

    /// <summary>
    /// Gets the Item collection, indexed by <see cref="Item.ItemID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Item> Items { get; set; }

    /// <summary>
    /// Gets the ItemCategory collection, indexed by <see cref="ItemCategory.ItemCategoryID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, ItemCategory> ItemCategories { get; set; }

    /// <summary>
    /// Gets the Weapon collection, indexed by <see cref="Weapon.WeaponID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Weapon> Weapons { get; set; }

    /// <summary>
    /// Gets the World collection, indexed by <see cref="World.WorldID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, World> Worlds { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsContext"/> class.
    /// </summary>
    public CollectionsContext()
    {
        CollectionInfos = new List<CollectionInfo>();
        Currencies = new Dictionary<uint, Currency>();
        Experiences = new Dictionary<uint, Experience>();
        Factions = new Dictionary<uint, Faction>();
        FireGroups = new Dictionary<uint, FireGroup>();
        FireModes = new Dictionary<uint, FireMode>();
        FireModeToProjectileMap = new Dictionary<uint, FireModeToProjectile>();
        Items = new Dictionary<uint, Item>();
        ItemCategories = new Dictionary<uint, ItemCategory>();
        Weapons = new Dictionary<uint, Weapon>();
        Worlds = new Dictionary<uint, World>();
    }

    /// <summary>
    /// Builds the <see cref="CollectionInfos"/> list.
    /// </summary>
    public void BuildCollectionInfos()
    {
        SnakeCaseJsonNamingPolicy nameConverter = new();
        List<CollectionInfo> collectionInfos = new();
        void AddCollection<T>(IReadOnlyDictionary<uint, T> collection)
        {
            CollectionInfo info = new
            (
                nameConverter.ConvertName(typeof(T).Name),
                collection.Count
            );
            collectionInfos.Add(info);
        }

        AddCollection(Currencies);
        AddCollection(Experiences);
        AddCollection(Factions);
        AddCollection(FireGroups);
        AddCollection(FireModes);
        AddCollection(FireModeToProjectileMap);
        AddCollection(Items);
        AddCollection(ItemCategories);
        AddCollection(Weapons);
        AddCollection(Worlds);

        CollectionInfos = collectionInfos;
    }
}

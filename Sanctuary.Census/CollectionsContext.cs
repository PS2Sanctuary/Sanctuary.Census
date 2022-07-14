using Sanctuary.Census.Models;
using System.Collections.Generic;

namespace Sanctuary.Census;

/// <summary>
/// Maintains a store of the DTO collections.
/// </summary>
public class CollectionsContext
{
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
    /// Gets the Item collection, indexed by <see cref="Item.ItemID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Item> Items { get; set; }

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
        Currencies = new Dictionary<uint, Currency>();
        Factions = new Dictionary<uint, Faction>();
        Items = new Dictionary<uint, Item>();
        Weapons = new Dictionary<uint, Weapon>();
        Worlds = new Dictionary<uint, World>();
    }
}

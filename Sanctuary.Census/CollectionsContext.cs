using Sanctuary.Census.Common.Objects.DtoModels;
using System.Collections.Generic;

namespace Sanctuary.Census;

/// <summary>
/// Maintains a store of the DTO collections.
/// </summary>
public class CollectionsContext
{
    /// <summary>
    /// Gets the Item collection, indexed by <see cref="Item.ItemID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Item> Items { get; set; }

    /// <summary>
    /// Gets the Weapon collection, indexed by <see cref="Weapon.WeaponID"/>.
    /// </summary>
    public IReadOnlyDictionary<uint, Weapon> Weapons { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsContext"/> class.
    /// </summary>
    public CollectionsContext()
    {
        Items = new Dictionary<uint, Item>();
        Weapons = new Dictionary<uint, Weapon>();
    }
}

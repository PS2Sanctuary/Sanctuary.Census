using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between a vehicle loadout slot and an item class that can be used in the slot.
/// </summary>
/// <param name="LoadoutId">The ID of the vehicle loadout that the slot belongs to.</param>
/// <param name="SlotId">The ID of the loadout slot.</param>
/// <param name="ItemClassId">The ID of the item class.</param>
[Collection]
[Description("A mapping between vehicle loadout slots and the item classes that can be used in the slot.")]
public record VehicleLoadoutSlotToItemClass
(
    [property: JoinKey] uint LoadoutId,
    [property: JoinKey] uint SlotId,
    [property: JoinKey] uint ItemClassId
) : ISanctuaryCollection;

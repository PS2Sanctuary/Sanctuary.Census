using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents mapping data between vehicle tint item classes and the loadout slots they are usable in.
/// </summary>
/// <param name="LoadoutId">The ID of the loadout.</param>
/// <param name="SlotId">The ID of the slot.</param>
/// <param name="TintItemClass">The ID of the tint item class.</param>
[Datasheet]
public partial record VehicleLoadoutSlotTintItemClass
(
    uint LoadoutId,
    uint SlotId,
    uint TintItemClass
);

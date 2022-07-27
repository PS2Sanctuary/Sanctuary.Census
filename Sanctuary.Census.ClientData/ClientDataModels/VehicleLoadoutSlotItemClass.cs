namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a mapping between loadout slots and item classes.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout.</param>
/// <param name="SlotID">The ID of the slot.</param>
/// <param name="ItemClass">The class of items that can be used in this slot.</param>
/// <param name="FlagLocked">Unknown.</param>
public record VehicleLoadoutSlotItemClass
(
    uint LoadoutID,
    uint SlotID,
    uint ItemClass,
    bool FlagLocked
);

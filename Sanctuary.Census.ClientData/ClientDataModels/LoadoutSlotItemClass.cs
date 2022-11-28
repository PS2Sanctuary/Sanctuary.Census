using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents data mapping item classes to the loadout slots that they are usable in.
/// </summary>
/// <param name="LoadoutId">The ID of the loadout.</param>
/// <param name="Slot">The ID of the loadout slot.</param>
/// <param name="ItemClass">The ID of the item class.</param>
/// <param name="FlagLocked">Unknown.</param>
/// <param name="ClientRequirementId">The client requirement that must be satisfied for the item class to be usable in the slot.</param>
/// <param name="ServerRequirementId">The server requirement that must be satisfied for the item class to be usable in the slot.</param>
[Datasheet]
public partial record LoadoutSlotItemClass
(
    uint LoadoutId,
    uint Slot,
    uint ItemClass,
    bool FlagLocked,
    uint ClientRequirementId,
    uint ServerRequirementId
);

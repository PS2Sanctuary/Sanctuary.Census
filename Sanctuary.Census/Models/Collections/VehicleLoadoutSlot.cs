using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a slot of a vehicle loadout.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout that this slot belongs to.</param>
/// <param name="SlotID">The ID of the slot.</param>
/// <param name="Name">The slot's name.</param>
/// <param name="Description">The description.</param>
/// <param name="ImageSetID">The ID of the slot's image set.</param>
/// <param name="FlagAutoEquip">Whether this slot is automatically equipped.</param>
/// <param name="FlagRequired">Whether this slot is required to be equipped.</param>
/// <param name="FlagIsVisible">Whether this slot is visible to the user.</param>
/// <param name="UiTag">The UI section that this slot appears under.</param>
[Collection]
public record VehicleLoadoutSlot
(
    [property: Key] uint LoadoutID,
    [property: Key] uint SlotID,
    LocaleString? Name,
    LocaleString? Description,
    uint? ImageSetID,
    bool FlagAutoEquip,
    bool FlagRequired,
    bool FlagIsVisible,
    string? UiTag
);

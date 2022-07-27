using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a vehicle attachment.
/// </summary>
/// <param name="ItemID">The ID of the attachment item.</param>
/// <param name="VehicleLoadoutID">The ID of a vehicle loadout that the attachment can be used on.</param>
/// <param name="VehicleID">The ID of a vehicle that the attachment can be used on.</param>
/// <param name="FactionID">The ID of a faction that the attachment can be used on.</param>
/// <param name="VehicleLoadoutSlotID">The vehicle loadout slot that the attachment can be used in.</param>
/// <param name="Description">The locale string ID of the vehicle's name.</param>
[Collection]
public record VehicleAttachment
(
    [property: Key] uint ItemID,
    [property: Key] uint VehicleLoadoutID,
    uint VehicleID,
    uint FactionID,
    uint VehicleLoadoutSlotID,
    LocaleString? Description
);

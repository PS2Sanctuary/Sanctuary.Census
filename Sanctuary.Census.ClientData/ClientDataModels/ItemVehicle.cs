using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client vehicle attachment data.
/// </summary>
/// <param name="ItemID">The ID of the item.</param>
/// <param name="VehicleID">The ID of a vehicle that the attachment can be used on.</param>
/// <param name="VehicleNameID">The locale string ID of the vehicle's name.</param>
/// <param name="FactionID">The ID of a faction that the attachment can be used on.</param>
/// <param name="VehicleLoadoutID">The ID of a loadout that the attachment can be used on.</param>
[Datasheet]
public partial record ItemVehicle
(
    uint ItemID,
    uint VehicleID,
    uint VehicleNameID,
    uint FactionID,
    uint VehicleLoadoutID
);

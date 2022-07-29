namespace Sanctuary.Census.PatchData.PatchDataModels;

/// <summary>
/// Represents static facility info.
/// </summary>
/// <param name="ZoneDefinition">The zone that the facility is on.</param>
/// <param name="ZoneInstance">The instance of the zone.</param>
/// <param name="FacilityID">The ID of the facility.</param>
/// <param name="FacilityNameID">The name of the facility.</param>
/// <param name="FacilityType">The type of the facility.</param>
/// <param name="LocationX">The X location of the facility.</param>
/// <param name="LocationY">The Y location of the facility.</param>
/// <param name="LocationZ">The Z location of the facility.</param>
public record StaticFacilityInfo
(
    ushort ZoneDefinition,
    ushort ZoneInstance,
    uint FacilityID,
    uint FacilityNameID,
    byte FacilityType,
    float LocationX,
    float LocationY,
    float LocationZ
);

namespace Sanctuary.Census.PatchData.PatchDataModels;

/// <summary>
/// Represents map region patch data.
/// </summary>
/// <param name="FacilityID">The ID of the facility within this region.</param>
/// <param name="ZoneID">The ID of the zone that this region is on.</param>
/// <param name="RegionID">The ID of the region.</param>
/// <param name="Name">The name of the facility contained within the region.</param>
/// <param name="TypeID">The ID of the type of the facility.</param>
/// <param name="TypeName">The name of the type of the facility.</param>
/// <param name="LocationX">The X coordinate of the center of the region.</param>
/// <param name="LocationY">The Y coordinate of the center of the region.</param>
/// <param name="LocationZ">The Z coordinate of the center of the region.</param>
public record MapRegionPatch
(
    uint FacilityID,
    uint ZoneID,
    uint RegionID,
    string Name,
    uint TypeID,
    string TypeName,
    float? LocationX,
    float? LocationY,
    float? LocationZ
);

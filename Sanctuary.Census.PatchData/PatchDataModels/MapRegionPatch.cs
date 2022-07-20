namespace Sanctuary.Census.PatchData.PatchDataModels;

/// <summary>
/// Represents map region patch data.
/// </summary>
/// <param name="FacilityID"></param>
/// <param name="ZoneID"></param>
/// <param name="RegionID"></param>
/// <param name="Name"></param>
/// <param name="TypeID"></param>
/// <param name="TypeName"></param>
/// <param name="LocationX"></param>
/// <param name="LocationY"></param>
/// <param name="LocationZ"></param>
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

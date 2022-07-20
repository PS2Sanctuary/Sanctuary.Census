namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents map region data.
/// </summary>
/// <param name="MapRegionId">The ID of the region.</param>
/// <param name="ZoneId">The ID of the zone that this region is on.</param>
/// <param name="FacilityId">The ID of the facility within this region.</param>
/// <param name="FacilityName">The name of the facility contained within the region.</param>
/// <param name="FacilityTypeId">The ID of the type of the facility.</param>
/// <param name="FacilityType">The name of the type of the facility.</param>
/// <param name="LocationX">The X coordinate of the center of the region.</param>
/// <param name="LocationY">The Y coordinate of the center of the region.</param>
/// <param name="LocationZ">The Z coordinate of the center of the region.</param>
public record MapRegion
(
    uint MapRegionId,
    uint ZoneId,
    uint FacilityId,
    string FacilityName,
    uint FacilityTypeId,
    string FacilityType,
    float? LocationX,
    float? LocationY,
    float? LocationZ
);

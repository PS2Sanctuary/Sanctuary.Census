using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents map region data.
/// </summary>
/// <param name="MapRegionId">The ID of the region.</param>
/// <param name="ZoneId">The ID of the zone that this region is on.</param>
/// <param name="FacilityId">The ID of the facility within this region.</param>
/// <param name="FacilityName">The name of the facility contained within the region.</param>
/// <param name="LocalizedFacilityName">The localized name of the facility contained within the region.</param>
/// <param name="FacilityTypeId">The ID of the type of the facility.</param>
/// <param name="FacilityType">The name of the type of the facility.</param>
/// <param name="LocationX">The X coordinate of the center of the region.</param>
/// <param name="LocationY">The Y coordinate of the center of the region.</param>
/// <param name="LocationZ">The Z coordinate of the center of the region.</param>
[Collection]
public record MapRegion
(
    [property: Key] uint MapRegionId,
    [property: Key] uint ZoneId,
    [property: Key] uint? FacilityId,
    string? FacilityName,
    LocaleString? LocalizedFacilityName,
    uint FacilityTypeId,
    string? FacilityType,
    decimal? LocationX,
    decimal? LocationY,
    decimal? LocationZ
) : ISanctuaryCollection;

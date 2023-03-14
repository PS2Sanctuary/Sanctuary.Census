using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

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
/// <param name="OutfitResourceRewardTypeDescription">The type of the Outfit Resource rewarded by this facility upon capture.</param>
/// <param name="OutfitResourceRewardAmount">The amount of the Outfit Resource rewarded by this facility upon capture.</param>
/// <param name="ImageSetID">The ID of the facility's image set.</param>
/// <param name="ImageID">The ID of the facility's default image.</param>
/// <param name="ImagePath">The relative path to the facility's default image.</param>
/// <param name="RewardCurrencyId">The currency that is awarded for capturing/owning facility.</param>
/// <param name="RewardAmount">The amount of currency that is awarded for capturing/owning the facility.</param>
[Collection]
public record MapRegion
(
    [property: JoinKey] uint MapRegionId,
    [property: JoinKey] uint ZoneId,
    [property: JoinKey] uint? FacilityId,
    string? FacilityName,
    LocaleString? LocalizedFacilityName,
    uint FacilityTypeId,
    string? FacilityType,
    decimal? LocationX,
    decimal? LocationY,
    decimal? LocationZ,
    string? OutfitResourceRewardTypeDescription,
    int? OutfitResourceRewardAmount,
    [property: JoinKey] uint? ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath,
    [property: JoinKey] uint? RewardCurrencyId,
    int? RewardAmount
) : ISanctuaryCollection;

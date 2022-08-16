using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents facility info.
/// </summary>
/// <param name="ZoneID">The zone that the facility is on.</param>
/// <param name="FacilityID">The ID of the facility.</param>
/// <param name="FacilityName">The name of the facility.</param>
/// <param name="FacilityTypeID">The type of the facility.</param>
/// <param name="LocationX">The X coordinate of the facility.</param>
/// <param name="LocationY">The Y coordinate of the facility.</param>
/// <param name="LocationZ">The Z coordinate of the facility.</param>
[Collection]
[Description("Information about in-game facilities. While missing some of the fields of map_region, this collection is guaranteed to be up-to-date")]
public record FacilityInfo
(
    [property: Key] ushort ZoneID,
    [property: Key] uint FacilityID,
    LocaleString FacilityName,
    byte FacilityTypeID,
    decimal LocationX,
    decimal LocationY,
    decimal LocationZ
);

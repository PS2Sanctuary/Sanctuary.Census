using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

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
public record FacilityInfo
(
    [property: Key] ushort ZoneID,
    [property: Key] uint FacilityID,
    LocaleString FacilityName,
    byte FacilityTypeID,
    float LocationX,
    float LocationY,
    float LocationZ
);

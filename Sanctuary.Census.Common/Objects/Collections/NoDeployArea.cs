using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a no-deployment area.
/// </summary>
/// <param name="AreaID">The ID of the area.</param>
/// <param name="ZoneID">The ID of the zone that the area is present on.</param>
/// <param name="Name">The name of the area.</param>
/// <param name="Type">The no-deployment type of the area.</param>
/// <param name="Shape">The shape of the area.</param>
/// <param name="LocationX1">The first X coordinate of the area.</param>
/// <param name="LocationY1">The first Y coordinate of the area.</param>
/// <param name="LocationZ1">The first Z coordinate of the area.</param>
/// <param name="Radius">The radius of the area. Present if the <paramref name="Shape"/> is a <c>sphere</c>.</param>
/// <param name="LocationX2">The second X coordinate of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="LocationY2">The second Y coordinate of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="LocationZ2">The second Z coordinate of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="RotationX">The X rotation of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="RotationY">The X rotation of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="RotationZ">The X rotation of the area. Present if the <paramref name="Shape"/> is a <c>box</c>.</param>
/// <param name="FacilityID">The ID of the facility that the area is linked to.</param>
[Collection]
public record NoDeployArea
(
    [property: Key] long AreaID, // In reality this is a uint32. Thanks Mongo
    [property: Key] uint ZoneID,
    string? Name,
    string Type,
    string Shape,
    decimal LocationX1,
    decimal LocationY1,
    decimal LocationZ1,
    decimal? Radius,
    decimal? LocationX2,
    decimal? LocationY2,
    decimal? LocationZ2,
    decimal? RotationX,
    decimal? RotationY,
    decimal? RotationZ,
    [property: Key] uint? FacilityID
) : ISanctuaryCollection;

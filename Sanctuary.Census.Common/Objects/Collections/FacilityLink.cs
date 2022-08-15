using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents facility link data.
/// </summary>
/// <param name="ZoneID">The ID of the zone that the facility is on.</param>
/// <param name="FacilityIdA">The first facility involved in the link.</param>
/// <param name="FacilityIdB">The second facility involved in the link.</param>
/// <param name="Description">The description of the link.</param>
[Collection]
public record FacilityLink
(
    [property:Key] uint ZoneID,
    [property:Key] int FacilityIdA,
    [property:Key] int FacilityIdB,
    string? Description
);

using Sanctuary.Census.Attributes;

namespace Sanctuary.Census.Models.Collections;

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
    uint ZoneID,
    int FacilityIdA,
    int FacilityIdB,
    string? Description
);

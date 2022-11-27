using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents facility link data.
/// </summary>
/// <param name="ZoneID">The ID of the zone that the facility is on.</param>
/// <param name="FacilityIdA">The first facility involved in the link.</param>
/// <param name="FacilityIdB">The second facility involved in the link.</param>
/// <param name="Description">The description of the link.</param>
[Collection]
[Description("Information about lattice links between facilities.")]
public record FacilityLink
(
    [property: JoinKey] uint ZoneID,
    int FacilityIdA,
    int FacilityIdB,
    string? Description
) : ISanctuaryCollection;

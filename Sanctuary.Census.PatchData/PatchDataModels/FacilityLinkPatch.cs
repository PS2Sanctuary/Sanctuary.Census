namespace Sanctuary.Census.PatchData.PatchDataModels;

/// <summary>
/// Represents facility link patch data.
/// </summary>
/// <param name="ZoneID">The ID of the zone that the facility is on.</param>
/// <param name="FacilityA">The first facility involved in the link.</param>
/// <param name="FacilityB">The second facility involved in the link.</param>
/// <param name="Description">The description of the link.</param>
public record FacilityLinkPatch
(
    uint ZoneID,
    int FacilityA,
    int FacilityB,
    string? Description
);

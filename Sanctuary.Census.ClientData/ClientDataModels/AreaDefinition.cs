namespace Sanctuary.Census.ClientData.ClientDataModels;

#pragma warning disable CS1591
public record AreaDefinition
(
    uint AreaID,
    string? Name,
    string Shape,
    decimal LocationX1,
    decimal LocationY1,
    decimal LocationZ1,
    decimal? LocationX2,
    decimal? LocationY2,
    decimal? LocationZ2,
    decimal? RotationX,
    decimal? RotationY,
    decimal? RotationZ,
    decimal? Radius,
    uint? FacilityID,
    uint RequirementID
);
#pragma warning restore CS1591

using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client vehicle skill set data.
/// </summary>
/// <param name="VehicleID">The ID of the vehicle.</param>
/// <param name="FactionID">The ID of the faction that the skill set is applied to the vehicle on.</param>
/// <param name="SkillSetID">The ID of the skill set.</param>
/// <param name="DisplayIndex">The display index.</param>
[Datasheet]
public partial record VehicleSkillSet
(
    uint VehicleID,
    uint FactionID,
    uint SkillSetID,
    byte DisplayIndex
);

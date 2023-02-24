using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client prestige level rank set data.
/// </summary>
/// <param name="Id">The ID of the rank set.</param>
/// <param name="Description">The description of the rank set.</param>
/// <param name="PrestigeLevelId">The ID of the prestige level the rank set maps to.</param>
[Datasheet]
public partial record PrestigeLevelRankSet
(
    uint Id,
    string Description,
    uint PrestigeLevelId
);

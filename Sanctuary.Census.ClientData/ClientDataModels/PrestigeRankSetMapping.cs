using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client data mapping experience ranks to the prestige level they are compatible with.
/// </summary>
/// <param name="Id">The ID of the mapping.</param>
/// <param name="PrestigeLevelRankSetId">The ID of the prestige rank set.</param>
/// <param name="ExperienceRankId">The ID of the experience rank.</param>
[Datasheet]
public partial record PrestigeRankSetMapping
(
    uint Id,
    uint PrestigeLevelRankSetId,
    uint ExperienceRankId
);

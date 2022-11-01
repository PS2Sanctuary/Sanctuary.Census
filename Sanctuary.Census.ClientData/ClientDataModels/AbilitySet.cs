using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a client ability set mapping.
/// </summary>
/// <param name="AbilitySetId">The ID of the set.</param>
/// <param name="AbilityId">The ID of the ability.</param>
/// <param name="OrderIndex">The order of the ability within the set.</param>
[Datasheet]
public partial record AbilitySet
(
    uint AbilitySetId,
    uint AbilityId,
    byte OrderIndex
);

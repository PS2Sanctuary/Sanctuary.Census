using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a client ability set mapping.
/// </summary>
/// <param name="AbilitySetId">The ID of the set.</param>
/// <param name="AbilityId">The ID of the ability.</param>
/// <param name="OrderIndex">The order of the ability within the set.</param>
[Collection]
public record AbilitySet
(
    [property: JoinKey] uint AbilitySetId,
    [property: JoinKey] uint AbilityId,
    byte OrderIndex
) : ISanctuaryCollection;

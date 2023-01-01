using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents population data for a world.
/// </summary>
/// <param name="WorldId">The ID of the world.</param>
/// <param name="LastUpdated">The time at which the entry was last updated.</param>
/// <param name="Total">The total number of players on the world.</param>
/// <param name="Population">The total number of players per faction on the world.</param>
[Collection]
public record WorldPopulation
(
    [property: JoinKey] uint WorldId,
    long LastUpdated,
    int Total,
    ValueEqualityDictionary<FactionDefinition, int> Population
) : ISanctuaryCollection;

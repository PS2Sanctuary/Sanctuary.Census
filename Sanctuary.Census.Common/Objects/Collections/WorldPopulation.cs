using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents population data for a world.
/// </summary>
/// <param name="WorldId">The ID of the world.</param>
/// <param name="Timestamp">The time at which the population entry was generated.</param>
/// <param name="Total">The total number of players on the world.</param>
/// <param name="Population">The total number of players per faction on the world.</param>
[Collection]
public record WorldPopulation
(
    [property: JoinKey] uint WorldId,
    long Timestamp,
    int Total,
    ValueEqualityDictionary<FactionDefinition, int> Population
) : IRealtimeCollection;

using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents population data for a zone.
/// </summary>
/// <param name="WorldId">The ID of the world that the zone is on.</param>
/// <param name="ZoneId">The ID of the zone.</param>
/// <param name="ZoneInstance">The instance of the zone.</param>
/// <param name="Timestamp">The time at which the population entry was generated.</param>
/// <param name="Total">The total number of players on the zone.</param>
/// <param name="Population">The total number of players per faction on the zone.</param>
[Collection(IsHidden = true, HideReason = "Calculation methodology is innacurate")]
public record ZonePopulation
(
    [property: JoinKey] uint WorldId,
    [property: JoinKey] ushort ZoneId,
    ushort ZoneInstance,
    long Timestamp,
    int Total,
    ValueEqualityDictionary<FactionDefinition, int> Population
) : IRealtimeCollection;

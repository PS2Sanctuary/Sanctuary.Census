using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Contains the maximum number of characters that can join a zone.
/// </summary>
/// <param name="WorldId">The ID of the world that the zone belongs to.</param>
/// <param name="ZoneId">The ID of the zone.</param>
/// <param name="Total">The total number of character that can join the zone.</param>
/// <param name="PopulationLimits">The per-faction character limit.</param>
[Collection]
[Description("Contains the maximum number of characters that can join a zone.")]
public record ZonePopulationLimits
(
    [property: JoinKey] uint WorldId,
    [property: JoinKey] ushort ZoneId,
    int Total,
    ValueEqualityDictionary<FactionDefinition, int> PopulationLimits
) : ISanctuaryCollection;

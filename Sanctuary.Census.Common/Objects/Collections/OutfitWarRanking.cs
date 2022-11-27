using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an outfit war ranking.
/// </summary>
/// <param name="RoundID">The ID of the round that this ranking is for.</param>
/// <param name="OutfitID">The ID of the outfit that this ranking is for.</param>
/// <param name="FactionID">The faction that the outfit is playing for.</param>
/// <param name="OutfitWarID">The ID of the outfit war for which the ranking applies to.</param>
/// <param name="Order">The base order of the outfit.</param>
/// <param name="RankingParameters">The ranking parameters.</param>
[Collection]
[Description("Contains rankings of an outfit within an outfit war. This collection is keyed by the primary_round_id of an outfit_war_rounds record.")]
public record OutfitWarRanking
(
    [property: JoinKey] ulong RoundID,
    [property: JoinKey] ulong OutfitID,
    [property: JoinKey] byte FactionID,
    [property: JoinKey] uint OutfitWarID,
    uint Order,
    ValueEqualityDictionary<string, int> RankingParameters
) : ISanctuaryCollection;

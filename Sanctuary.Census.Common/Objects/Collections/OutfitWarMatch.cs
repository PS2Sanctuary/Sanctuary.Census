using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an outfit war match.
/// </summary>
/// <param name="MatchID">The ID of the match.</param>
/// <param name="RoundID">The ID of the round that the match belongs to.</param>
/// <param name="OutfitWarID">The ID of the war that the match belongs to.</param>
/// <param name="OutfitAId">The ID of the first participating outfit.</param>
/// <param name="OutfitBId">The ID of the second participating outfit.</param>
/// <param name="StartTime">The start time of the match.</param>
/// <param name="Order">The display order of the match.</param>
/// <param name="WorldID">The ID of the world that the match will taking place on.</param>
/// <param name="OutfitAFactionId">The faction ID of the first participating outfit.</param>
/// <param name="OutfitBFactionId">The faction ID of the second participating outfit.</param>
[Collection]
[Description("Contains information about outfit war matches, including start times and matched outfits")]
public record OutfitWarMatch
(
    ulong MatchID,
    [property: Key] ulong? RoundID,
    [property: Key] uint OutfitWarID,
    ulong OutfitAId,
    ulong OutfitBId,
    ulong StartTime,
    uint Order,
    [property: Key] uint WorldID,
    uint OutfitAFactionId,
    uint OutfitBFactionId
) : ISanctuaryCollection;

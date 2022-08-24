using Sanctuary.Census.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an outfit war ranking.
/// </summary>
/// <param name="RoundID">The ID of the round that this ranking is for.</param>
/// <param name="OutfitID">The ID of the outfit that this ranking is for.</param>
/// <param name="FactionID">The faction that the outfit is playing for.</param>
/// <param name="Order">The base order of the outfit.</param>
/// <param name="RankingParameters">The ranking parameters.</param>
[Collection]
[Description("Contains rankings of an outfit within an outfit war. This collection is keyed by the primary_round_id of an outfit_war_rounds record.")]
public record OutfitWarRanking
(
    [property: Key] ulong RoundID,
    [property: Key] ulong OutfitID,
    [property: Key] byte FactionID,
    uint Order,
    Dictionary<string, uint> RankingParameters
)
{
    /// <inheritdoc />
    public virtual bool Equals(OutfitWarRanking? other)
        => other is not null
           && RoundID.Equals(other.RoundID)
           && OutfitID.Equals(other.OutfitID)
           && FactionID.Equals(other.FactionID)
           && Order.Equals(other.Order)
           && RankingParameters.SequenceEqual(other.RankingParameters);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(RoundID);
        hashCode.Add(OutfitID);
        hashCode.Add(FactionID);
        hashCode.Add(Order);

        foreach ((string key, uint value) in RankingParameters)
        {
            hashCode.Add(key);
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }
}

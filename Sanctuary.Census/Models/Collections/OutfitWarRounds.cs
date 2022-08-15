using Sanctuary.Census.Attributes;
using Sanctuary.Zone.Packets.OutfitWars;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents information about an outfit war's rounds.
/// </summary>
/// <param name="OutfitWarID">The ID of the war that the rounds belong to.</param>
/// <param name="PrimaryRoundID">The ID of the war's primary round.</param>
/// <param name="Rounds">The rounds.</param>
[Collection]
public record OutfitWarRounds
(
    [property: Key] uint OutfitWarID,
    [property: Key] ulong PrimaryRoundID,
    IReadOnlyList<OutfitWarRounds.Round> Rounds
)
{
    /// <summary>
    /// Represents information about an outfit war round.
    /// </summary>
    /// <param name="Order">The order of the round among its siblings.</param>
    /// <param name="Stage">The stage that the round belongs to.</param>
    /// <param name="StartTime">The start time of the round.</param>
    /// <param name="EndTime">The end time of the round.</param>
    public record Round
    (
        uint Order,
        Stage Stage,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime
    );
}

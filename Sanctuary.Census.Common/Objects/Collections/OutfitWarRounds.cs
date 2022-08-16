using Sanctuary.Census.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

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
    /// Enumerates the stages of an outfit war.
    /// </summary>
    public enum RoundStage : uint
    {
        /// <summary>
        /// Represents info about the overall war.
        /// </summary>
        OverallWar = 0,

        /// <summary>
        /// A qualifier stage.
        /// </summary>
        Qualifier = 1,

        /// <summary>
        /// A playoff stage.
        /// </summary>
        Playoff = 2,

        /// <summary>
        /// A final stage.
        /// </summary>
        Final = 3
    };

    /// <summary>
    /// Represents information about an outfit war round.
    /// </summary>
    /// <param name="Order">The order of the round among its siblings.</param>
    /// <param name="Stage">The stage that the round belongs to.</param>
    /// <param name="StartTime">The start time of the round, as a unix seconds timestamp.</param>
    /// <param name="EndTime">The end time of the round, as a unix seconds timestamp.</param>
    [Collection]
    public record Round
    (
        uint Order,
        RoundStage Stage,
        ulong StartTime,
        ulong EndTime
    );
}

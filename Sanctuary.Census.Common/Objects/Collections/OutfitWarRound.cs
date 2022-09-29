using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an outfit war round.
/// </summary>
/// <param name="RoundID">The ID of the round.</param>
/// <param name="OutfitWarID">The ID of the war that the round belongs to.</param>
/// <param name="PrimaryRoundID">The ID of the round's parent.</param>
/// <param name="Order">The order of the round among its siblings in the same war.</param>
/// <param name="Stage">The round stage.</param>
/// <param name="StartTime">The start time of the round, as a unix seconds timestamp.</param>
/// <param name="EndTime">The end time of the round, as a unix seconds timestamp.</param>
[Collection]
[Description("OBSOLETE: This collection has been superseded by outfit_war_rounds and will be deprecated after Nexus Season One ends")]
public record OutfitWarRound
(
    [property: Key] ulong RoundID,
    [property: Key] uint OutfitWarID,
    ulong PrimaryRoundID,
    uint Order,
    OutfitWarRound.RoundStage Stage,
    ulong StartTime,
    ulong EndTime
) : ISanctuaryCollection
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
}

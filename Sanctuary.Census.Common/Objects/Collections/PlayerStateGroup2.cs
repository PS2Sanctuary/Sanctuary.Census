using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents player movement state data.
/// </summary>
/// <param name="PlayerStateGroupId">The ID of the group that this state belongs to.</param>
/// <param name="PlayerStateId">The ID of the state.</param>
/// <param name="CanIronSight">Indicates whether the player can ADS while in this state.</param>
/// <param name="CofGrowRate">
/// The rate in degrees/sec at which the player's cone of fire increases to a given value if it is currently smaller.
/// </param>
/// <param name="CofMax">The maximum size, in degrees, of the player's cone of fire.</param>
/// <param name="CofMin">The minimum size, in degrees, of the player's cone of fire.</param>
/// <param name="CofRecoveryDelayMs">
/// The delay in milliseconds before the player's cone of fire begins reducing to the <paramref name="CofMin"/>.
/// Any growth in the COF will reset this delay.
/// </param>
/// <param name="CofRecoveryRate">
/// The rate in degrees/sec at which player's cone of fire reduces to <paramref name="CofMin"/>.
/// </param>
/// <param name="CofShotsBeforePenalty"></param>
/// <param name="CofRecoveryDelayThreshold"></param>
/// <param name="CofTurnPenalty"></param>
[Collection]
public record PlayerStateGroup2
(
    [property:Key] uint PlayerStateGroupId,
    [property:Key] uint PlayerStateId,
    bool CanIronSight,
    decimal CofGrowRate,
    decimal CofMax,
    decimal CofMin,
    ushort CofRecoveryDelayMs,
    decimal CofRecoveryRate,
    byte CofShotsBeforePenalty,
    uint CofRecoveryDelayThreshold,
    decimal CofTurnPenalty
) : ISanctuaryCollection;

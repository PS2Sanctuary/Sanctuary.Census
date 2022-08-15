using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

#pragma warning disable CS1591
[Collection]
public record PlayerStateGroup2
(
    [property:Key] uint PlayerStateGroupId,
    [property:Key] uint PlayerStateId,
    bool CanIronSight,
    float CofGrowRate,
    float CofMax,
    float CofMin,
    ushort CofRecoveryDelayMs,
    float CofRecoveryRate,
    byte CofShotsBeforePenalty,
    uint CofRecoveryDelayThreshold,
    float CofTurnPenalty
);
#pragma warning restore CS1591

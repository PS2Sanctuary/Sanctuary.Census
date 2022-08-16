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
    decimal CofGrowRate,
    decimal CofMax,
    decimal CofMin,
    ushort CofRecoveryDelayMs,
    decimal CofRecoveryRate,
    byte CofShotsBeforePenalty,
    uint CofRecoveryDelayThreshold,
    decimal CofTurnPenalty
);
#pragma warning restore CS1591

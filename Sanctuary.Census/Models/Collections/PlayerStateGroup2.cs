using Sanctuary.Census.Attributes;

namespace Sanctuary.Census.Models.Collections;

#pragma warning disable CS1591
[Collection]
public record PlayerStateGroup2
(
    uint PlayerStateGroupId,
    uint PlayerStateId,
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

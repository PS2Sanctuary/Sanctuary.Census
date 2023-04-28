using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client resource data.
/// </summary>
/// <param name="Id"></param>
/// <param name="ResourceType"></param>
/// <param name="NameId"></param>
/// <param name="DescriptionId"></param>
/// <param name="ActivatedAbilityId"></param>
/// <param name="DepletedAbilityId"></param>
/// <param name="RemovedAbilityId"></param>
/// <param name="ActivateCompEffectId"></param>
/// <param name="TerminateCompEffectId"></param>
/// <param name="DepletedCompEffectId"></param>
/// <param name="InitialValue"></param>
/// <param name="InitialValueMax"></param>
/// <param name="UseInitialValueAsMax"></param>
/// <param name="MaxValue"></param>
/// <param name="ValueMarkerLo"></param>
/// <param name="ValueMarkerMed"></param>
/// <param name="ValueMarkerHi"></param>
/// <param name="RegenPerMs"></param>
/// <param name="RegenDelayMs"></param>
/// <param name="RegenDamageInterruptMs"></param>
/// <param name="RegenTickMsec"></param>
/// <param name="BurnPerMs"></param>
/// <param name="BurnTickMsec"></param>
/// <param name="BurnDelayMs"></param>
/// <param name="FlagProfileScope"></param>
/// <param name="FlagNotVehMountScope"></param>
/// <param name="PacketBroadcastRange"></param>
/// <param name="IconId"></param>
/// <param name="ReplicateAll"></param>
/// <param name="ReplicateLocal"></param>
/// <param name="AllowBurnHealXp"></param>
/// <param name="FullAbilityId"></param>
/// <param name="FullCompEffectId"></param>
[Datasheet]
public partial record Resource
(
    uint Id,
    uint ResourceType,
    uint NameId,
    uint DescriptionId,
    uint ActivatedAbilityId,
    uint DepletedAbilityId,
    uint RemovedAbilityId,
    uint ActivateCompEffectId,
    uint TerminateCompEffectId,
    uint DepletedCompEffectId,
    decimal InitialValue,
    decimal InitialValueMax,
    bool UseInitialValueAsMax,
    decimal MaxValue,
    decimal ValueMarkerLo,
    decimal ValueMarkerMed,
    decimal ValueMarkerHi,
    decimal RegenPerMs,
    uint RegenDelayMs,
    uint RegenDamageInterruptMs,
    uint RegenTickMsec,
    decimal BurnPerMs,
    uint BurnTickMsec,
    uint BurnDelayMs,
    bool FlagProfileScope,
    bool FlagNotVehMountScope,
    uint PacketBroadcastRange,
    uint IconId,
    bool ReplicateAll,
    bool ReplicateLocal,
    bool AllowBurnHealXp,
    uint FullAbilityId,
    uint FullCompEffectId
);

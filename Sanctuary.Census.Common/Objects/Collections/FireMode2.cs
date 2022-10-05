using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents fire mode data - the firing characteristics of a weapon when in a certain state, e.g. hipfire.
/// </summary>
/// <param name="FireModeID">The ID of the fire mode.</param>
/// <param name="FireModeTypeID">The type of the fire mode.</param>
/// <param name="Description">A description of the fire mode.</param>
/// <param name="AbilityID">The ID of the ability that the fire mode applies.</param>
/// <param name="AmmoItemID"></param>
/// <param name="AmmoSlot"></param>
/// <param name="ArmorPenetration"></param>
/// <param name="Automatic"></param>
/// <param name="BulletArcKickAngle">The relative pitch angle in degrees at which a bullet exits the barrel.</param>
/// <param name="GriefImmune">Indicates whether grief points can be accumulated when the fire mode is active.</param>
/// <param name="LaserGuided">Indicates whether projectiles will follow the crosshair when the fire mode is active.</param>
/// <param name="IronSights"></param>
/// <param name="SprintFire">Indicates whether the fire mode is usable while sprinting.</param>
/// <param name="SwayCanSteady">Indicates whether any scope sway applied by the fire mode can be steadied.</param>
/// <param name="UseInWater">Indicates whether the fire mode is usable underwater.</param>
/// <param name="CloakAfterFireDelayMs">The delay in milliseconds before the infiltrator's cloak ability is usable, after firing.</param>
/// <param name="CofOverride"></param>
/// <param name="CofPelletSpread"></param>
/// <param name="CofRange"></param>
/// <param name="CofRecoil"></param>
/// <param name="CofScalar"></param>
/// <param name="CofScalarMoving"></param>
/// <param name="DamageHeadMultiplier"></param>
/// <param name="DamageLegsMultiplier"></param>
/// <param name="DamageIndirectEffectID"></param>
/// <param name="FanAngle">The angle in degrees at which pellets such as those on the Horizon, are separated by.</param>
/// <param name="FireAmmoPerShot"></param>
/// <param name="FireAutoFireMs"></param>
/// <param name="FireBurstCount"></param>
/// <param name="FireChargeUpMs"></param>
/// <param name="FireCooldownDurationMs"></param>
/// <param name="FireDelayMs"></param>
/// <param name="FireDurationMs"></param>
/// <param name="FireDetectRange"></param>
/// <param name="FireNeedsLock">Indicates whether a lock on a target is required before the fire mode can be used.</param>
/// <param name="FirePelletsPerShot"></param>
/// <param name="FireRefireMs"></param>
/// <param name="HeatPerShot"></param>
/// <param name="HeatRecoveryDelayMs"></param>
/// <param name="HeatThreshold"></param>
/// <param name="MaxDamage"></param>
/// <param name="MaxDamageRange"></param>
/// <param name="MaxDamageInd"></param>
/// <param name="MaxDamageIndRadius"></param>
/// <param name="MinDamage"></param>
/// <param name="MinDamageRange"></param>
/// <param name="MinDamageInd"></param>
/// <param name="MinDamageIndRadius"></param>
/// <param name="MoveModifier">The movement speed multiplier applied by the fire mode.</param>
/// <param name="TurnModifier">The turn speed multiplier applied by the fire mode.</param>
/// <param name="PlayerStateGroupID"></param>
/// <param name="ProjectileSpeedOverride"></param>
/// <param name="RecoilAngleMax"></param>
/// <param name="RecoilAngleMin"></param>
/// <param name="RecoilFirstShotModifier"></param>
/// <param name="RecoilHorizontalMax"></param>
/// <param name="RecoilHorizontalMaxIncrease"></param>
/// <param name="RecoilHorizontalMin"></param>
/// <param name="RecoilHorizontalMinIncrease"></param>
/// <param name="RecoilHorizontalTolerance"></param>
/// <param name="RecoilIncrease"></param>
/// <param name="RecoilIncreaseCrouched"></param>
/// <param name="RecoilMagnitudeMax"></param>
/// <param name="RecoilMagnitudeMin"></param>
/// <param name="RecoilMaxTotalMagnitude"></param>
/// <param name="RecoilRecoveryAcceleration"></param>
/// <param name="RecoilRecoveryDelayMs"></param>
/// <param name="RecoilRecoveryRate"></param>
/// <param name="RecoilShotsAtMinMagnitude"></param>
/// <param name="ReloadAmmoFillMs"></param>
/// <param name="ReloadBlockAuto"></param>
/// <param name="ReloadChamberMs"></param>
/// <param name="ReloadContinuous"></param>
/// <param name="ReloadLoopStartMs"></param>
/// <param name="ReloadLoopEndMs"></param>
/// <param name="ReloadTimeMs"></param>
/// <param name="ShieldBypassPct"></param>
/// <param name="SprintAfterFireDelayMs">The delay in milliseconds before sprinting can occur, after firing.</param>
/// <param name="SwayAmplitudeX"></param>
/// <param name="SwayAmplitudeY"></param>
/// <param name="SwayPeriodX"></param>
/// <param name="SwayPeriodY"></param>
/// <param name="ZoomDefault"></param>
[Collection]
[Description("Specific properties of a weapon's firing characteristics")]
public record FireMode2
(
    [property:Key] uint FireModeID,
    byte FireModeTypeID,
    LocaleString Description,
    [property: Key] uint? AbilityID,
    uint? AmmoItemID,
    byte AmmoSlot,
    bool ArmorPenetration,
    bool Automatic,
    decimal BulletArcKickAngle,
    bool GriefImmune,
    bool LaserGuided,
    bool IronSights,
    bool SprintFire,
    bool SwayCanSteady,
    bool UseInWater,
    int? CloakAfterFireDelayMs,
    decimal CofOverride,
    decimal CofPelletSpread,
    decimal CofRange,
    decimal CofRecoil,
    decimal CofScalar,
    decimal CofScalarMoving,
    decimal? DamageHeadMultiplier,
    decimal? DamageLegsMultiplier,
    uint? DamageIndirectEffectID,
    decimal? FanAngle,
    byte FireAmmoPerShot,
    ushort FireAutoFireMs,
    byte FireBurstCount,
    ushort FireChargeUpMs,
    ushort FireCooldownDurationMs,
    ushort FireDelayMs,
    ushort FireDurationMs,
    ushort FireDetectRange,
    bool FireNeedsLock,
    byte FirePelletsPerShot,
    ushort FireRefireMs,
    uint? HeatPerShot,
    ushort? HeatRecoveryDelayMs,
    uint? HeatThreshold,
    int MaxDamage,
    int MaxDamageRange,
    int MaxDamageInd,
    decimal MaxDamageIndRadius,
    int MinDamage,
    int MinDamageRange,
    int MinDamageInd,
    decimal MinDamageIndRadius,
    decimal MoveModifier,
    decimal TurnModifier,
    [property: Key] uint? PlayerStateGroupID,
    decimal? ProjectileSpeedOverride,
    decimal RecoilAngleMax,
    decimal RecoilAngleMin,
    decimal RecoilFirstShotModifier,
    decimal? RecoilHorizontalMax,
    decimal? RecoilHorizontalMaxIncrease,
    decimal? RecoilHorizontalMin,
    decimal? RecoilHorizontalMinIncrease,
    decimal? RecoilHorizontalTolerance,
    decimal? RecoilIncrease,
    decimal? RecoilIncreaseCrouched,
    decimal? RecoilMagnitudeMax,
    decimal? RecoilMagnitudeMin,
    decimal? RecoilMaxTotalMagnitude,
    decimal? RecoilRecoveryAcceleration,
    ushort? RecoilRecoveryDelayMs,
    decimal? RecoilRecoveryRate,
    byte? RecoilShotsAtMinMagnitude,
    ushort? ReloadAmmoFillMs,
    bool ReloadBlockAuto,
    ushort? ReloadChamberMs,
    bool ReloadContinuous,
    ushort? ReloadLoopStartMs,
    ushort? ReloadLoopEndMs,
    ushort ReloadTimeMs,
    int ShieldBypassPct,
    int? SprintAfterFireDelayMs,
    decimal? SwayAmplitudeX,
    decimal? SwayAmplitudeY,
    decimal? SwayPeriodX,
    decimal? SwayPeriodY,
    decimal ZoomDefault
) : ISanctuaryCollection;

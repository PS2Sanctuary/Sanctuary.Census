using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

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
/// <param name="AnimKickMagnitude"></param>
/// <param name="AnimRecoilMagnitude"></param>
/// <param name="ArmorPenetration"></param>
/// <param name="Automatic"></param>
/// <param name="BulletArcKickAngle">The relative pitch angle in degrees at which a bullet exits the barrel.</param>
/// <param name="GriefImmune">Indicates whether grief points can be accumulated when the fire mode is active.</param>
/// <param name="LaserGuided">Indicates whether projectiles will follow the crosshair when the fire mode is active.</param>
/// <param name="IronSights"></param>
/// <param name="SprintFire">Indicates whether the fire mode is usable while sprinting.</param>
/// <param name="SwayCanSteady">Indicates whether any scope sway applied by the fire mode can be steadied.</param>
/// <param name="UseInWater">Indicates whether the fire mode is usable underwater.</param>
/// <param name="AbilityAfterFireDelayMs">The delay in milliseconds before a profile's ability is usable, after firing.</param>
/// <param name="CanLock">Indicates whether a lock-on weapon can obtain a lock while the fire mode is active.</param>
/// <param name="CofOverride"></param>
/// <param name="CofPelletSpread"></param>
/// <param name="CofRange"></param>
/// <param name="CofRecoil"></param>
/// <param name="CofScalar"></param>
/// <param name="CofScalarMoving"></param>
/// <param name="DamageHeadMultiplier"></param>
/// <param name="DamageLegsMultiplier"></param>
/// <param name="DamageIndirectEffectID"></param>
/// <param name="DeployAnimTimeMs">
/// Time time taken by the deployment animation of a relevant weapon, e.g. the Shield Recharging Device. May have additional functionality.
/// </param>
/// <param name="FanAngle">
/// The angle in degrees at which fan-based pellets, such as those on the VE-C Horizon, diverge from their rotational axis at.
/// </param>
/// <param name="FanAngleRotateDegrees">
/// The degree of rotation in the XY plane (that is, the viewing plane of the user) at which fan-based pellets are rotated.
/// </param>
/// <param name="FanConicalSpread">
/// The maximum distance at which fan-based pellets are spread from each other, by means of a dedicated 'cone of fire' for each pellet.
/// </param>
/// <param name="FireAmmoPerShot"></param>
/// <param name="FireAutoFireMs"></param>
/// <param name="FireBurstCount"></param>
/// <param name="FireChargeMinimumMs">The minimum amount of time that a weapon such as the Scorpion or Corsair catapult must be charged for.</param>
/// <param name="FireChargeUpMs"></param>
/// <param name="FireCooldownDurationMs"></param>
/// <param name="FireDelayMs"></param>
/// <param name="FireDurationMs"></param>
/// <param name="FireDetectRange"></param>
/// <param name="FireNeedsLock">Indicates whether a lock on a target is required before the fire mode can be used.</param>
/// <param name="FirePelletsPerShot"></param>
/// <param name="FireRefireMs"></param>
/// <param name="FireRequirementExpression">
/// An expression defining the conditions that must be met for firing to be enabled.
/// </param>
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
/// <param name="ReloadBlockAuto">
/// Better know as BlockAutoReload. Indicates whether the fire mode prevents the weapon from automatically
/// reloading when the magazine is depleted.
/// </param>
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
/// <param name="TargetRequirementExpression">
/// An expression defining the type of target the user must be aiming at for firing to be enabled.
/// </param>
/// <param name="ZoomDefault"></param>
[Collection]
[Description("Specific properties of a weapon's firing characteristics")]
public record FireMode2
(
    [property: JoinKey] uint FireModeID,
    byte FireModeTypeID,
    LocaleString Description,
    [property: JoinKey] uint? AbilityID,
    uint? AmmoItemID,
    byte AmmoSlot,
    decimal? AnimKickMagnitude,
    decimal? AnimRecoilMagnitude,
    bool ArmorPenetration,
    bool Automatic,
    decimal BulletArcKickAngle,
    bool GriefImmune,
    bool LaserGuided,
    bool IronSights,
    bool SprintFire,
    bool SwayCanSteady,
    bool UseInWater,
    int? AbilityAfterFireDelayMs,
    bool CanLock,
    decimal CofOverride,
    decimal CofPelletSpread,
    decimal CofRange,
    decimal CofRecoil,
    decimal CofScalar,
    decimal CofScalarMoving,
    decimal? DamageHeadMultiplier,
    decimal? DamageLegsMultiplier,
    uint? DamageIndirectEffectID,
    ushort? DeployAnimTimeMs,
    decimal? FanAngle,
    decimal? FanAngleRotateDegrees,
    decimal? FanConicalSpread,
    byte FireAmmoPerShot,
    ushort FireAutoFireMs,
    byte FireBurstCount,
    ushort? FireChargeMinimumMs,
    ushort FireChargeUpMs,
    ushort FireCooldownDurationMs,
    ushort FireDelayMs,
    ushort FireDurationMs,
    ushort FireDetectRange,
    bool FireNeedsLock,
    byte FirePelletsPerShot,
    ushort FireRefireMs,
    string? FireRequirementExpression,
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
    [property: JoinKey] uint? PlayerStateGroupID,
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
    short? RecoilRecoveryDelayMs,
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
    string? TargetRequirementExpression,
    decimal ZoomDefault
) : ISanctuaryCollection;

using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireMode2"/> collection.
/// </summary>
public class FireModeCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        CollectionsContext context
    )
    {
        if (serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        if (clientDataCache.FireModeDisplayStats.Count == 0)
            throw new MissingCacheDataException(typeof(FireModeDisplayStat));

        Dictionary<uint, FireModeDisplayStat> clientDisplayStats = new();
        foreach (FireModeDisplayStat fmds in clientDataCache.FireModeDisplayStats)
            clientDisplayStats[fmds.ID] = fmds;

        Dictionary<uint, FireMode2> builtFireModes = new();
        foreach (FireMode fireMode in serverDataCache.WeaponDefinitions.FireModes)
        {
            localeDataCache.TryGetLocaleString(fireMode.TypeDescriptionID, out LocaleString? description);
            clientDisplayStats.TryGetValue(fireMode.ID, out FireModeDisplayStat? displayStats);

            FireMode2 built = new
            (
                fireMode.ID,
                fireMode.FireModeTypeID,
                description!,
                fireMode.AbilityID == 0 ? null : fireMode.AbilityID,
                fireMode.AmmoItemID == 0 ? null : fireMode.AmmoItemID,
                fireMode.AmmoSlot,
                displayStats?.ArmorPenetration ?? false,
                (fireMode.Flags & FireModeFlags.Automatic) != 0,
                (fireMode.Flags & FireModeFlags.GriefImmune) != 0,
                (fireMode.Flags & FireModeFlags.LaserGuided) != 0,
                (fireMode.Flags & FireModeFlags.IronSights) != 0,
                (fireMode.Flags & FireModeFlags.SprintFire) != 0,
                (fireMode.Flags & FireModeFlags.SwayCanSteady) != 0,
                (fireMode.Flags & FireModeFlags.UseInWater) != 0,
                fireMode.CofOverride,
                fireMode.CofPelletSpread,
                fireMode.CofRange,
                fireMode.CofRecoil,
                fireMode.CofScalar,
                fireMode.CofScalarMoving,
                fireMode.DamageHeadMultiplier == 0 ? null : fireMode.DamageHeadMultiplier,
                fireMode.DamageLegsMultiplier == 0 ? null : fireMode.DamageLegsMultiplier,
                fireMode.DamageIndirectEffectID == 0 ? null : fireMode.DamageIndirectEffectID,
                fireMode.FireAmmoPerShot,
                fireMode.FireAutoFireMs,
                fireMode.FireBurstCount,
                fireMode.FireChargeUpMs,
                fireMode.FireCooldownDurationMs,
                fireMode.FireDelayMs,
                fireMode.FireDurationMs,
                fireMode.FireDetectRange,
                fireMode.FirePelletsPerShot,
                fireMode.FireRefireMs,
                fireMode.HeatPerShot == 0 ? null : fireMode.HeatPerShot,
                fireMode.HeatRecoveryDelayMS == 0 ? null : fireMode.HeatRecoveryDelayMS,
                fireMode.HeatThreshold == 0 ? null : fireMode.HeatThreshold,
                displayStats?.MaxDamage ?? 0,
                displayStats?.MaxDamageRange ?? 0,
                displayStats?.MaxDamageInd ?? 0,
                displayStats?.MaxDamageIndRadius ?? 0,
                displayStats?.MinDamage ?? 0,
                displayStats?.MinDamageRange ?? 0,
                displayStats?.MinDamageInd ?? 0,
                displayStats?.MinDamageIndRadius ?? 0,
                fireMode.MoveModifier,
                fireMode.TurnModifier,
                fireMode.PlayerStateGroupID == 0 ? null : fireMode.PlayerStateGroupID,
                fireMode.ProjectileSpeedOverride == 0 ? null : fireMode.ProjectileSpeedOverride,
                fireMode.RecoilAngleMax = fireMode.RecoilAngleMax,
                fireMode.RecoilAngleMin = fireMode.RecoilAngleMin,
                fireMode.RecoilFirstShotModifier,
                fireMode.RecoilHorizontalMax == 0 ? null : fireMode.RecoilHorizontalMax,
                fireMode.RecoilHorizontalMaxIncrease == 0 ? null : fireMode.RecoilHorizontalMaxIncrease,
                fireMode.RecoilHorizontalMin == 0 ? null : fireMode.RecoilHorizontalMin,
                fireMode.RecoilHorizontalMinIncrease == 0 ? null : fireMode.RecoilHorizontalMinIncrease,
                fireMode.RecoilHorizontalTolerance == 0 ? null : fireMode.RecoilHorizontalTolerance,
                fireMode.RecoilIncrease == 0 ? null : fireMode.RecoilIncrease,
                fireMode.RecoilIncreaseCrouched == 0 ? null : fireMode.RecoilIncreaseCrouched,
                fireMode.RecoilMagnitudeMax == 0 ? null : fireMode.RecoilMagnitudeMax,
                fireMode.RecoilMagnitudeMin == 0 ? null : fireMode.RecoilMagnitudeMin,
                fireMode.RecoilMaxTotalMagnitude == 0 ? null : fireMode.RecoilMaxTotalMagnitude,
                fireMode.RecoilRecoveryAcceleration == 0 ? null : fireMode.RecoilRecoveryAcceleration,
                fireMode.RecoilRecoveryDelayMs == 0 ? null : fireMode.RecoilRecoveryDelayMs,
                fireMode.RecoilRecoveryRate == 0 ? null : fireMode.RecoilRecoveryRate,
                fireMode.RecoilShotsAtMinMagnitude == 0 ? null : fireMode.RecoilShotsAtMinMagnitude,
                fireMode.ReloadAmmoFillMs == 0 ? null : fireMode.ReloadAmmoFillMs,
                (fireMode.Flags & FireModeFlags.ReloadBlockAuto) != 0,
                fireMode.ReloadChamberMs == 0 ? null : fireMode.ReloadChamberMs,
                (fireMode.Flags & FireModeFlags.ReloadContinuous) != 0,
                fireMode.ReloadLoopStartTimeMs == 0 ? null : fireMode.ReloadLoopStartTimeMs,
                fireMode.ReloadLoopEndTimeMs == 0 ? null : fireMode.ReloadLoopEndTimeMs,
                fireMode.ReloadTimeMs,
                displayStats?.ShieldBypassPct ?? 0,
                fireMode.SwayAmplitudeX == 0 ? null : fireMode.SwayAmplitudeX,
                fireMode.SwayAmplitudeY == 0 ? null : fireMode.SwayAmplitudeY,
                fireMode.SwayPeriodX == 0 ? null : fireMode.SwayPeriodX,
                fireMode.SwayPeriodY == 0 ? null : fireMode.SwayPeriodY,
                fireMode.ZoomDefault
            );
            builtFireModes.TryAdd(built.FireModeID, built);
        }

        context.FireModes = builtFireModes;
    }
}

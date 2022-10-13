using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireMode2"/> collection.
/// </summary>
public class FireModeCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireModeCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public FireModeCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        if (_clientDataCache.FireModeDisplayStats is null)
            throw new MissingCacheDataException(typeof(FireModeDisplayStat));

        Dictionary<uint, FireModeDisplayStat> clientDisplayStats = new();
        foreach (FireModeDisplayStat fmds in _clientDataCache.FireModeDisplayStats)
            clientDisplayStats[fmds.ID] = fmds;

        Dictionary<uint, FireMode2> builtFireModes = new();
        foreach (FireMode fireMode in _serverDataCache.WeaponDefinitions.FireModes)
        {
            _localeDataCache.TryGetLocaleString(fireMode.TypeDescriptionID, out LocaleString? description);
            clientDisplayStats.TryGetValue(fireMode.ID, out FireModeDisplayStat? displayStats);

            FireMode2 built = new
            (
                fireMode.ID,
                fireMode.FireModeTypeID,
                description!,
                fireMode.AbilityID.ToNullableUInt(),
                fireMode.AmmoItemID.ToNullableUInt(),
                fireMode.AmmoSlot,
                fireMode.AnimKickMagnitude.ToNullableDecimal(),
                fireMode.AnimRecoilMagnitude.ToNullableDecimal(),
                displayStats?.ArmorPenetration ?? false,
                (fireMode.Flags & FireModeFlags.Automatic) != 0,
                new decimal(fireMode.BulletArcKickAngle),
                (fireMode.Flags & FireModeFlags.GriefImmune) != 0,
                (fireMode.Flags & FireModeFlags.LaserGuided) != 0,
                (fireMode.Flags & FireModeFlags.IronSights) != 0,
                (fireMode.Flags & FireModeFlags.SprintFire) != 0,
                (fireMode.Flags & FireModeFlags.SwayCanSteady) != 0,
                (fireMode.Flags & FireModeFlags.UseInWater) != 0,
                fireMode.CloakAfterFireDelayMs.ToNullableInt(),
                new decimal(fireMode.CofOverride),
                new decimal(fireMode.CofPelletSpread),
                new decimal(fireMode.CofRange),
                new decimal(fireMode.CofRecoil),
                new decimal(fireMode.CofScalar),
                new decimal(fireMode.CofScalarMoving),
                fireMode.DamageHeadMultiplier.ToNullableDecimal(),
                fireMode.DamageLegsMultiplier.ToNullableDecimal(),
                fireMode.DamageIndirectEffectID.ToNullableUInt(),
                fireMode.FanAngleDegrees.ToNullableDecimal(),
                fireMode.FanAngleRotateDegrees.ToNullableDecimal(),
                fireMode.FanConicalSpread.ToNullableDecimal(),
                fireMode.FireAmmoPerShot,
                fireMode.FireAutoFireMs,
                fireMode.FireBurstCount,
                fireMode.FireChargeMinimumMs.ToNullableUShort(),
                fireMode.FireChargeUpMs,
                fireMode.FireCooldownDurationMs,
                fireMode.FireDelayMs,
                fireMode.FireDurationMs,
                fireMode.FireDetectRange,
                (fireMode.Flags & FireModeFlags.FireNeedsLock) != 0,
                fireMode.FirePelletsPerShot,
                fireMode.FireRefireMs,
                fireMode.HeatPerShot.ToNullableUInt(),
                fireMode.HeatRecoveryDelayMS.ToNullableUShort(),
                fireMode.HeatThreshold.ToNullableUInt(),
                displayStats?.MaxDamage ?? 0,
                displayStats?.MaxDamageRange ?? 0,
                displayStats?.MaxDamageInd ?? 0,
                new decimal(displayStats?.MaxDamageIndRadius ?? 0),
                displayStats?.MinDamage ?? 0,
                displayStats?.MinDamageRange ?? 0,
                displayStats?.MinDamageInd ?? 0,
                new decimal(displayStats?.MinDamageIndRadius ?? 0),
                new decimal(fireMode.MoveModifier),
                new decimal(fireMode.TurnModifier),
                fireMode.PlayerStateGroupID == 0 ? null : fireMode.PlayerStateGroupID,
                fireMode.ProjectileSpeedOverride.ToNullableDecimal(),
                new decimal(fireMode.RecoilAngleMax),
                new decimal(fireMode.RecoilAngleMin),
                new decimal(fireMode.RecoilFirstShotModifier),
                fireMode.RecoilHorizontalMax.ToNullableDecimal(),
                fireMode.RecoilHorizontalMaxIncrease.ToNullableDecimal(),
                fireMode.RecoilHorizontalMin.ToNullableDecimal(),
                fireMode.RecoilHorizontalMinIncrease.ToNullableDecimal(),
                fireMode.RecoilHorizontalTolerance.ToNullableDecimal(),
                fireMode.RecoilIncrease.ToNullableDecimal(),
                fireMode.RecoilIncreaseCrouched.ToNullableDecimal(),
                fireMode.RecoilMagnitudeMax.ToNullableDecimal(),
                fireMode.RecoilMagnitudeMin.ToNullableDecimal(),
                fireMode.RecoilMaxTotalMagnitude.ToNullableDecimal(),
                fireMode.RecoilRecoveryAcceleration.ToNullableDecimal(),
                fireMode.RecoilRecoveryDelayMs.ToNullableShort(),
                fireMode.RecoilRecoveryRate.ToNullableDecimal(),
                fireMode.RecoilShotsAtMinMagnitude.ToNullableByte(),
                fireMode.ReloadAmmoFillMs.ToNullableUShort(),
                (fireMode.Flags & FireModeFlags.ReloadBlockAuto) != 0,
                fireMode.ReloadChamberMs.ToNullableUShort(),
                (fireMode.Flags & FireModeFlags.ReloadContinuous) != 0,
                fireMode.ReloadLoopStartTimeMs.ToNullableUShort(),
                fireMode.ReloadLoopEndTimeMs.ToNullableUShort(),
                fireMode.ReloadTimeMs,
                displayStats?.ShieldBypassPct ?? 0,
                fireMode.SprintAfterFireDelayMs.ToNullableInt(),
                fireMode.SwayAmplitudeX.ToNullableDecimal(),
                fireMode.SwayAmplitudeY.ToNullableDecimal(),
                fireMode.SwayPeriodX.ToNullableDecimal(),
                fireMode.SwayPeriodY.ToNullableDecimal(),
                new decimal(fireMode.ZoomDefault)
            );
            builtFireModes.TryAdd(built.FireModeID, built);
        }

        await dbContext.UpsertCollectionAsync(builtFireModes.Values, ct).ConfigureAwait(false);
    }
}

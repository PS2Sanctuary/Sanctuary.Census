using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Weapon"/> collection.
/// </summary>
public class WeaponCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public WeaponCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
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

        Dictionary<uint, Weapon> builtWeapons = new();
        foreach (WeaponDefinition definition in _serverDataCache.WeaponDefinitions.Definitions)
        {
            _localeDataCache.TryGetLocaleString(definition.RangeDescriptionID, out LocaleString? rangeDescription);

            string? animWieldTypeName = definition.AnimationWieldTypeName.Length == 0
                ? null
                : definition.AnimationWieldTypeName;

            Weapon built = new
            (
                definition.WeaponID,
                definition.GroupID == 0 ? null : definition.GroupID,
                definition.EquipMs,
                definition.UnequipMs,
                definition.ToIronSightsMs,
                definition.FromIronSightsMs,
                definition.ToIronSightsAnimMs,
                definition.FromIronSightsAnimMs,
                definition.SprintRecoveryMs,
                new decimal(definition.TurnRateModifier),
                new decimal(definition.MoveSpeedModifier),
                definition.HeatBleedoffRate == 0 ? null : definition.HeatBleedoffRate,
                definition.HeatOverheatPenaltyMs == 0 ? null : definition.HeatOverheatPenaltyMs,
                rangeDescription,
                definition.MeleeDetectWidth == 0 ? null : new decimal(definition.MeleeDetectWidth),
                definition.MeleeDetectHeight == 0 ? null : new decimal(definition.MeleeDetectHeight),
                animWieldTypeName,
                definition.MinPitch == 0 ? null : new decimal(definition.MinPitch),
                definition.MaxPitch == 0 ? null : new decimal(definition.MaxPitch),
                (definition.Flags & WeaponFlags.EquipNeedsAmmo) != 0,
                definition.NextUseDelayMs
            );
            builtWeapons.TryAdd(built.WeaponId, built);
        }

        await dbContext.UpsertCollectionAsync(builtWeapons.Values, ct).ConfigureAwait(false);
    }
}

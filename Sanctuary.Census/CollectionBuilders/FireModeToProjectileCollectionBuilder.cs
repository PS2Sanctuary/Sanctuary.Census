using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireModeToProjectile"/> collection.
/// </summary>
public class FireModeToProjectileCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        Dictionary<uint, FireModeToProjectile> builtMaps = serverDataCache.WeaponDefinitions
            .FireModeToProjectileMaps
            .Select(map => new FireModeToProjectile(map.FireModeID, map.ProjectileID))
            .ToDictionary(built => built.FireModeID);

        context.FireModeToProjectileMap = builtMaps;
    }
}

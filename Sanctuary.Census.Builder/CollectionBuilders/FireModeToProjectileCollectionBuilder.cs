using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireModeToProjectile"/> collection.
/// </summary>
public class FireModeToProjectileCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireModeToProjectileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public FireModeToProjectileCollectionBuilder
    (
        IServerDataCacheService serverDataCache
    )
    {
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

        IEnumerable<FireModeToProjectile> builtMaps = _serverDataCache.WeaponDefinitions
            .FireModeToProjectileMaps
            .Select(map => new FireModeToProjectile(map.FireModeID, map.ProjectileID));

        await dbContext.UpsertFireModesToProjectilesAsync(builtMaps, ct);
    }
}

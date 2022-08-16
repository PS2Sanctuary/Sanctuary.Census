using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Projectile"/> collection.
/// </summary>
public class ProjectileCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public ProjectileCollectionBuilder
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
        if (_serverDataCache.ProjectileDefinitions is null)
            throw new MissingCacheDataException(typeof(ProjectileDefinitions));

        Dictionary<uint, Projectile> builtProjectiles = new();
        foreach (ProjectileDefinition projectile in _serverDataCache.ProjectileDefinitions.Projectiles)
        {
            Projectile built = new
            (
                projectile.ProjectileID,
                projectile.ProjectileFlightTypeID,
                projectile.Speed,
                projectile.SpeedMax == 0 ? null : projectile.SpeedMax,
                projectile.Acceleration == 0 ? null : projectile.Acceleration,
                projectile.TurnRate,
                projectile.Lifespan,
                projectile.Drag,
                projectile.Gravity,
                projectile.ProjectileRadiusMeters,
                projectile.LockonAcceleration == 0 ? null : projectile.LockonAcceleration,
                projectile.LockonLifespan == 0 ? null : projectile.LockonLifespan,
                projectile.ArmDistance == 0 ? null : projectile.ArmDistance,
                projectile.TetherDistance == 0 ? null : projectile.TetherDistance,
                projectile.DetonateDistance == 0 ? null : projectile.DetonateDistance,
                projectile.ProximityLockonRangeHalfMeters == 0 ? null : projectile.ProximityLockonRangeHalfMeters,
                (projectile.Flags & ProjectileFlags.Sticky) != 0,
                (projectile.Flags & ProjectileFlags.SticksToPlayer) != 0,
                (projectile.Flags & ProjectileFlags.DetonateOnContact) != 0,
                projectile.LockonLoseAngle == 0 ? null : projectile.LockonLoseAngle,
                (projectile.Flags & ProjectileFlags.LockonSeekInFlight1) != 0,
                projectile.ActorDefinition
            );
            builtProjectiles.Add(built.ProjectileId, built);
        }

        await dbContext.UpsertProjectilesAsync(builtProjectiles.Values, ct);
    }
}

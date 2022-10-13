using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System;
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
                projectile.Acceleration.ToNullableDecimal(),
                projectile.ActorDefinition,
                projectile.FpActorDefinition.Length == 0 ? null : projectile.FpActorDefinition,
                projectile.ArmDistance.ToNullableUShort(),
                projectile.DetonateDistance.ToNullableUShort(),
                (projectile.Flags & ProjectileFlags.DetonateOnContact) != 0,
                new decimal(projectile.Drag),
                new decimal(projectile.Gravity),
                new decimal(projectile.Lifespan),
                (projectile.Flags & ProjectileFlags.LifespanDetonate) != 0,
                projectile.LockonAcceleration.ToNullableDecimal(),
                projectile.LockonLifespan.ToNullableDecimal(),
                projectile.LockonLoseAngle.ToNullableUShort(),
                (projectile.Flags & ProjectileFlags.LockonSeekInFlight1) != 0,
                projectile.ProjectileFlightTypeID,
                new decimal(projectile.ProjectileRadiusMeters),
                projectile.ProximityLockonRangeHalfMeters.ToNullableDecimal(),
                new decimal(projectile.Speed),
                projectile.SpeedMax.ToNullableDecimal(),
                (projectile.Flags & ProjectileFlags.Sticky) != 0,
                (projectile.Flags & ProjectileFlags.SticksToPlayer) != 0,
                projectile.TetherDistance.ToNullableDecimal(),
                projectile.TracerFrequency.ToNullableByte(),
                projectile.FpTracerFrequency.ToNullableByte(),
                new decimal(projectile.TurnRate),
                new decimal(projectile.VelocityInheritScalar)
            );
            builtProjectiles.Add(built.ProjectileId, built);
        }

        await dbContext.UpsertCollectionAsync(builtProjectiles.Values, ct).ConfigureAwait(false);
    }
}

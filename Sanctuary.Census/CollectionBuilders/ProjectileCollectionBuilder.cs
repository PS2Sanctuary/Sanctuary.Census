using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Projectile"/> collection.
/// </summary>
public class ProjectileCollectionBuilder : ICollectionBuilder
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
        if (serverDataCache.ProjectileDefinitions is null)
            throw new MissingCacheDataException(typeof(ProjectileDefinitions));

        Dictionary<uint, Projectile> builtProjectiles = new();
        foreach (ProjectileDefinition projectile in serverDataCache.ProjectileDefinitions.Projectiles)
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
                projectile.LockonAcceleration == 0 ? null : projectile.LockonAcceleration,
                projectile.LockonLifespan == 0 ? null : projectile.LockonLifespan,
                projectile.ArmDistance == 0 ? null : projectile.ArmDistance,
                projectile.DetonateDistance == 0 ? null : projectile.DetonateDistance,
                (projectile.Flags & ProjectileFlags.Sticky) != 0,
                (projectile.Flags & ProjectileFlags.SticksToPlayer) != 0,
                (projectile.Flags & ProjectileFlags.DetonateOnContact) != 0,
                projectile.LockonLoseAngle == 0 ? null : projectile.LockonLoseAngle,
                (projectile.Flags & ProjectileFlags.LockonSeekInFlight1) != 0,
                projectile.ActorDefinition
            );
            builtProjectiles.Add(built.ProjectileId, built);
        }

        context.Projectiles = builtProjectiles;
    }
}

using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
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
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IRequirementsHelperService _requirementsHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="requirementsHelper">The requirements helper service.</param>
    public ProjectileCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        IRequirementsHelperService requirementsHelper
    )
    {
        _clientDataCache = clientDataCache;
        _serverDataCache = serverDataCache;
        _requirementsHelper = requirementsHelper;
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

        if (_clientDataCache.ClientRequirementExpressions is null)
            throw new MissingCacheDataException(typeof(ClientRequirementExpression));

        Dictionary<uint, Projectile> builtProjectiles = new();
        foreach (ProjectileDefinition projectile in _serverDataCache.ProjectileDefinitions.Projectiles)
        {
            _requirementsHelper.TryGetClientExpression
            (
                projectile.StickToTargetClientRequirementExpressionId,
                out string? stickTotargetRequirementExpression
            );

            _requirementsHelper.TryGetClientExpression
            (
                projectile.CanProximityLockTargetRequirementExpressionId,
                out string? canProximityLockTargetExpression
            );

            _requirementsHelper.TryGetClientExpression
            (
                projectile.CreateFlakExplosionRequirementExpressionId,
                out string? createFlakExplosionExpression
            );

            Projectile built = new
            (
                projectile.ProjectileID,
                projectile.Acceleration.ToNullableDecimal(),
                projectile.ActorDefinition,
                projectile.FpActorDefinition.Length == 0 ? null : projectile.FpActorDefinition,
                projectile.ArmDistance.ToNullableUShort(),
                projectile.BombletDetonationMaxRange.ToNullableDecimal(),
                projectile.BombletDetonationMinRange.ToNullableDecimal(),
                canProximityLockTargetExpression,
                createFlakExplosionExpression,
                projectile.DetonateDistance.ToNullableUShort(),
                (projectile.Flags & ProjectileFlags.DetonateOnContact) != 0,
                new decimal(projectile.Drag),
                new decimal(projectile.Gravity),
                new decimal(projectile.Lifespan),
                (projectile.Flags & ProjectileFlags.LifespanDetonate) != 0,
                projectile.LockonAcceleration.ToNullableDecimal(),
                projectile.LockonLifespan.ToNullableDecimal(),
                projectile.LockonLoseAngle.ToNullableUShort(),
                projectile.ProximityLockonRangeHalfMeters.ToNullableDecimal(),
                (projectile.Flags & ProjectileFlags.LockonSeekInFlight1) != 0,
                projectile.ProjectileFlightTypeID,
                FlightTypeToString(projectile.ProjectileFlightTypeID),
                new decimal(projectile.ProjectileRadiusMeters),
                new decimal(projectile.ProjectileRadiusMeters),
                projectile.ProximityLockonRangeHalfMeters.ToNullableDecimal(),
                new decimal(projectile.Speed),
                projectile.SpeedMax.ToNullableDecimal(),
                (projectile.Flags & ProjectileFlags.Sticky) != 0,
                (projectile.Flags & ProjectileFlags.SticksToPlayer) != 0,
                stickTotargetRequirementExpression,
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

    private static string? FlightTypeToString(byte flightTypeId)
        => (Projectile.FlightType)flightTypeId switch
        {
            Projectile.FlightType.Ballistic => nameof(Projectile.FlightType.Ballistic),
            Projectile.FlightType.TrueBallistic => nameof(Projectile.FlightType.TrueBallistic),
            _ => null
        };
}

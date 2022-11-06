using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents projectile data.
/// </summary>
/// <param name="ProjectileId">The ID of the projectile.</param>
/// <param name="Acceleration">The acceleration of the projectile.</param>
/// <param name="ActorDefinition">The model used to represent the projectile to a third-person observer.</param>
/// <param name="ActorDefinitionFirstPerson">The model used to represent the projectile.</param>
/// <param name="ArmDistance">The distance after which a projectile becomes 'active'. E.g., under-barrel grenades only explode after travelling 10m.</param>
/// <param name="DetonateDistance">The distance a projectile must travel before it can be detonated.</param>
/// <param name="DetonateOnContact">Indicates whether the projectile detonates on contact with a target.</param>
/// <param name="Drag">The force of drag on the projectile.</param>
/// <param name="Gravity">The force of gravity on the projectile.</param>
/// <param name="Lifespan">The lifespan of the projectile.</param>
/// <param name="LifespanDetonate">Indicates whether the projectile will detonate at the end of its <paramref name="Lifespan"/>.</param>
/// <param name="LockonAcceleration">Unknown.</param>
/// <param name="LockonLifespan">The amount of time for which a projectile will maintain a lock.</param>
/// <param name="LockonLoseAngle">The angle to target at which the projectile will lose its lock.</param>
/// <param name="LockonSeekInFlight">Indicates whether the projectile will lock-on to a target while in flight.</param>
/// <param name="ProjectileFlightTypeId">The flight type of the projectile.</param>
/// <param name="ProjectileRadiusMeters">The radius of the projectile in meters.</param>
/// <param name="ProximityLockonRangeHalfMeters">
/// The range, in half-meters (divide by two in order to match in-game meters), at which a proximity projectile will lock on to a target.
/// </param>
/// <param name="Speed">The cruise speed of the projectile.</param>
/// <param name="SpeedMax">The maximum speed of the projectile.</param>
/// <param name="Sticky">Indicates whether the projectile is sticky.</param>
/// <param name="SticksToPlayers">Indicates whether the projectile can stick to players.</param>
/// <param name="StickToTargetRequirementExpression">
/// An expression defining the type of target that the projectile can stick to.
/// </param>
/// <param name="TetherDistance">The distance that a projectile can travel from its origin entity.</param>
/// <param name="TracerFrequency">The number of projectiles that must be fired before a tracer is spawned for a third-person observer.</param>
/// <param name="TracerFrequencyFirstPerson">The number of projectiles that must be fired before a tracer is spawned.</param>
/// <param name="TurnRate">The rate at which the projectile can turn while in flight.</param>
/// <param name="VelocityInheritScalar">The magnitude of velocity that the projectile inherits from the player entity.</param>
[Collection]
public record Projectile
(
    [property: Key] uint ProjectileId,
    decimal? Acceleration,
    string ActorDefinition,
    string? ActorDefinitionFirstPerson,
    ushort? ArmDistance,
    ushort? DetonateDistance,
    bool DetonateOnContact,
    decimal Drag,
    decimal Gravity,
    decimal Lifespan,
    bool LifespanDetonate,
    decimal? LockonAcceleration,
    decimal? LockonLifespan,
    ushort? LockonLoseAngle,
    bool LockonSeekInFlight,
    byte ProjectileFlightTypeId,
    string? ProjectileFlightTypeDescription,
    decimal ProjectileRadiusMeters,
    decimal? ProximityLockonRangeHalfMeters,
    decimal Speed,
    decimal? SpeedMax,
    bool Sticky,
    bool SticksToPlayers,
    string? StickToTargetRequirementExpression,
    decimal? TetherDistance,
    byte? TracerFrequency,
    byte? TracerFrequencyFirstPerson,
    decimal TurnRate,
    decimal VelocityInheritScalar
) : ISanctuaryCollection
{
    /// <summary>
    /// Enumerates the various projectile flight types.
    /// </summary>
    public enum FlightType
    {
        /// <summary>
        /// The standard projectile flight model. 'Fakes being effected by gravity'.
        /// Details of implementation unknown.
        /// </summary>
        Ballistic = 1,

        /// <summary>
        /// True ballistic projectiles are properly effected by gravity based on the firing angle.
        /// </summary>
        TrueBallistic = 3
    }
}

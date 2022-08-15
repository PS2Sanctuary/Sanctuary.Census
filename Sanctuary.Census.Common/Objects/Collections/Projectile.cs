using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents projectile data.
/// </summary>
/// <param name="ProjectileId">The ID of the projectile.</param>
/// <param name="ProjectileFlightTypeId">The flight type of the projectile.</param>
/// <param name="Speed">The cruise speed of the projectile.</param>
/// <param name="SpeedMax">The maximum speed of the projectile.</param>
/// <param name="Acceleration">The acceleration of the projectile.</param>
/// <param name="TurnRate">The rate at which the projectile can turn while in flight.</param>
/// <param name="Lifespan">The lifespan of the projectile.</param>
/// <param name="Drag">The force of drag on the projectile.</param>
/// <param name="Gravity">The force of gravity on the projectile.</param>
/// <param name="ProjectileRadiusMeters">The radius of the projectile in meters.</param>
/// <param name="LockonAcceleration">Unknown.</param>
/// <param name="LockonLifespan">The amount of time for which a projectile will maintain a lock.</param>
/// <param name="ArmDistance">The distance after which a projectile becomes 'active'. E.g., under-barrel grenades only explode after travelling 10m.</param>
/// <param name="TetherDistance">The distance that a projectile can travel from its origin entity.</param>
/// <param name="DetonateDistance">The distance a projectile must travel before it can be detonated.</param>
/// <param name="ProximityLockonRangeHalfMeters">
/// The range, in half-meters (divide by two in order to match in-game meters), at which a proximity projectile will lock on to a target.
/// </param>
/// <param name="Sticky">Indicates whether the projectile is sticky.</param>
/// <param name="SticksToPlayers">Indicates whether the projectile can stick to players.</param>
/// <param name="DetonateOnContact">Indicates whether the projectile detonates on contact with a target.</param>
/// <param name="LockonLoseAngle">The angle to target at which the projectile will lose its lock.</param>
/// <param name="LockonSeekInFlight">Indicates whether the projectile will seek its lock target while in flight.</param>
/// <param name="ActorDefinition">The model used to represent the projectile.</param>
[Collection]
public record Projectile
(
    [property:Key] uint ProjectileId,
    byte ProjectileFlightTypeId,
    float Speed,
    float? SpeedMax,
    float? Acceleration,
    float TurnRate,
    float Lifespan,
    float Drag,
    float Gravity,
    float ProjectileRadiusMeters,
    float? LockonAcceleration,
    float? LockonLifespan,
    ushort? ArmDistance,
    float? TetherDistance,
    ushort? DetonateDistance,
    float? ProximityLockonRangeHalfMeters,
    bool Sticky,
    bool SticksToPlayers,
    bool DetonateOnContact,
    ushort? LockonLoseAngle,
    bool LockonSeekInFlight,
    string ActorDefinition
);

using Sanctuary.Census.Attributes;

namespace Sanctuary.Census.Models.Collections;

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
/// <param name="LockonAcceleration">Unknown.</param>
/// <param name="LockonLifespan">The amount of time for which a projectile will maintain a lock.</param>
/// <param name="ArmDistance">The distance after which a projectile becomes 'active'. E.g., under-barrel grenades only explode after travelling 10m.</param>
/// <param name="DetonateDistance">The distance a projectile must travel before it can be detonated.</param>
/// <param name="Sticky">Indicates whether the projectile is sticky.</param>
/// <param name="SticksToPlayers">Indicates whether the projectile can stick to players.</param>
/// <param name="DetonateOnContact">Indicates whether the projectile detonates on contact with a target.</param>
/// <param name="LockonLoseAngle">The angle to target at which the projectile will lose its lock.</param>
/// <param name="LockonSeekInFlight">Indicates whether the projectile will seek its lock target while in flight.</param>
/// <param name="ActorDefinition">The model used to represent the projectile.</param>
[Collection(PrimaryJoinField = nameof(ProjectileId))]
public record Projectile
(
    uint ProjectileId,
    byte ProjectileFlightTypeId,
    float Speed,
    float? SpeedMax,
    float? Acceleration,
    float TurnRate,
    float Lifespan,
    float Drag,
    float Gravity,
    float? LockonAcceleration,
    float? LockonLifespan,
    ushort? ArmDistance,
    ushort? DetonateDistance,
    bool Sticky,
    bool SticksToPlayers,
    bool DetonateOnContact,
    ushort? LockonLoseAngle,
    bool LockonSeekInFlight,
    string ActorDefinition
);

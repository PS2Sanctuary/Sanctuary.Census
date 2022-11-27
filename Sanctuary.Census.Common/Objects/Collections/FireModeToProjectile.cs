using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between a fire mode and a projectile.
/// </summary>
/// <param name="FireModeID">The ID of the fire mode.</param>
/// <param name="ProjectileID">The ID of the projectile.</param>
[Collection]
public record FireModeToProjectile
(
    [property: JoinKey] uint FireModeID,
    [property: JoinKey] uint ProjectileID
) : ISanctuaryCollection;

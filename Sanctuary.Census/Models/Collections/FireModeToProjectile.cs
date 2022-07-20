using Sanctuary.Census.Attributes;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a mapping between a fire mode and a projectile.
/// </summary>
/// <param name="FireModeID">The ID of the fire mode.</param>
/// <param name="ProjectileID">The ID of the projectile.</param>
[Collection]
public record FireModeToProjectile
(
    uint FireModeID,
    uint ProjectileID
);

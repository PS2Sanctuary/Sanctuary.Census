namespace Sanctuary.Census.Models;

/// <summary>
/// Represents a mapping between a fire mode and a projectile.
/// </summary>
/// <param name="FireModeID">The ID of the fire mode.</param>
/// <param name="ProjectileID">The ID of the projectile.</param>
public record FireModeToProjectile
(
    uint FireModeID,
    uint ProjectileID
);

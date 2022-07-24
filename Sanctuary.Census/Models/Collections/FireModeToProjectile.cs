using Sanctuary.Census.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a mapping between a fire mode and a projectile.
/// </summary>
/// <param name="FireModeID">The ID of the fire mode.</param>
/// <param name="ProjectileID">The ID of the projectile.</param>
[Collection(PrimaryJoinField = nameof(FireModeToProjectile.FireModeID))]
public record FireModeToProjectile
(
    [property:Key] uint FireModeID,
    [property:Key] uint ProjectileID
);

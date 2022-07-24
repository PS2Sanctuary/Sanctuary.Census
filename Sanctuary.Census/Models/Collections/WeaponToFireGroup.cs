using Sanctuary.Census.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a mapping between a <see cref="Weapon"/> and a <see cref="FireGroup"/>.
/// </summary>
/// <param name="WeaponId">The ID of the weapon.</param>
/// <param name="FireGroupId">The ID of the fire group.</param>
/// <param name="FireGroupIndex">The index of the fire group within the mapping list.</param>
[Collection(PrimaryJoinField = nameof(WeaponToFireGroup.WeaponId))]
public record WeaponToFireGroup
(
    [property:Key] uint WeaponId,
    [property:Key] uint FireGroupId,
    uint FireGroupIndex
);

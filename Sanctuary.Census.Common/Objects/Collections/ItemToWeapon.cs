using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an item-to-weapon mapping.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="WeaponId">The ID of the weapon.</param>
[Collection]
public record ItemToWeapon
(
    [property:Key] uint ItemId,
    [property:Key] uint WeaponId
);

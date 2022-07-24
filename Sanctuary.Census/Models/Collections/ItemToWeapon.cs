using Sanctuary.Census.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents an item-to-weapon mapping.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="WeaponId">The ID of the weapon.</param>
[Collection(PrimaryJoinField = nameof(ItemToWeapon.ItemId))]
public record ItemToWeapon
(
    [property:Key] uint ItemId,
    [property:Key] uint WeaponId
);

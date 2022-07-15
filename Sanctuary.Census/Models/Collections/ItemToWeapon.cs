﻿namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents an item-to-weapon mapping.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="WeaponId">The ID of the weapon.</param>
public record ItemToWeapon
(
    uint ItemId,
    uint WeaponId
);

﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents <see cref="Weapon"/>'s ammo slot.
/// </summary>
/// <param name="WeaponId">The ID of the weapon that this ammo slot belongs to.</param>
/// <param name="WeaponSlotIndex">The index of the ammo slot within the mapping list.</param>
/// <param name="ClipSize">The size of a single clip of this ammo slot.</param>
/// <param name="Capacity">The size of the reserve ammunition capacity of this ammo slot.</param>
/// <param name="ClipModelName">The name of the model that represents this clip.</param>
[Collection]
public record WeaponAmmoSlot
(
    [property: JoinKey] uint WeaponId,
    uint WeaponSlotIndex,
    short ClipSize,
    ushort Capacity,
    string? ClipModelName
) : ISanctuaryCollection;

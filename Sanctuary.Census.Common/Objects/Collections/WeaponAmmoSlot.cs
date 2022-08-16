using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

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
    [property:Key] uint WeaponId,
    [property:Key] uint WeaponSlotIndex,
    ushort ClipSize,
    ushort Capacity,
    string? ClipModelName
);

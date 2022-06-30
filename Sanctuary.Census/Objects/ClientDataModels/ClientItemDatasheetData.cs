namespace Sanctuary.Census.Objects.ClientDataModels;

/// <summary>
/// Represents client display data for weaponry.
/// </summary>
/// <param name="ItemID">The item ID of the weapon.</param>
/// <param name="DatasheetID">The datasheet ID of the weapon.</param>
/// <param name="WeaponID">The weapon ID.</param>
/// <param name="FireGroupID">The fire group ID of the weapon.</param>
/// <param name="DirectDamage">The direct damage amount of the weapon.</param>
/// <param name="IndirectDamage">The indirect damage amount of the weapon.</param>
/// <param name="DamageFalloff">The damage falloff of the weapon.</param>
/// <param name="RefireTimeMS">The time in milliseconds that the weapon takes to recover after firing a projectile.</param>
/// <param name="ReloadTimeMS">The time in milliseconds that the weapon takes to reload.</param>
/// <param name="ClipSize">The clip size of the weapon.</param>
/// <param name="RangeStringID">The string ID of the general range category that the weapon falls into.</param>
/// <param name="MinConeOfFire">The weapon's starting cone of fire.</param>
public record ClientItemDatasheetData
(
    uint ItemID,
    uint DatasheetID,
    uint WeaponID,
    uint FireGroupID,
    int DirectDamage,
    int IndirectDamage,
    int DamageFalloff,
    ushort RefireTimeMS,
    ushort ReloadTimeMS,
    ushort ClipSize,
    uint RangeStringID,
    float MinConeOfFire
);

namespace Sanctuary.Census.Objects.Models;

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

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client fire mode stats.
/// </summary>
/// <param name="ID">The ID of the fire mode.</param>
/// <param name="MaxDamage">The maximum damage of the fire mode.</param>
/// <param name="MaxDamageRange">The maximum range at which the fire mode will deal its <paramref name="MaxDamage"/>.</param>
/// <param name="MinDamage">The minimum damage of the fire mode.</param>
/// <param name="MinDamageRange">The range at, and beyond which the fire mode will deal its <paramref name="MinDamage"/>.</param>
/// <param name="ShieldBypassPct">The percentage of damage dealt by the fire mode that bypasses shields.</param>
/// <param name="ArmorPenetration">Indicates whether the fire mode can penetrate armor.</param>
/// <param name="MaxDamageInd">The maximum indirect damage of the fire mode.</param>
/// <param name="MaxDamageIndRadius">The maximum radius at which the fire mode will deals its <paramref name="MaxDamageInd"/>.</param>
/// <param name="MinDamageInd">The minimum indirect damage of the fire mode.</param>
/// <param name="MinDamageIndRadius">The radius at, and beyond which the fire mode will deal its <paramref name="MinDamageInd"/>.</param>
public record FireModeDisplayStat
(
    uint ID,
    int MaxDamage,
    int MaxDamageRange,
    int MinDamage,
    int MinDamageRange,
    int ShieldBypassPct,
    bool ArmorPenetration,
    int MaxDamageInd,
    float MaxDamageIndRadius,
    int MinDamageInd,
    float MinDamageIndRadius
);

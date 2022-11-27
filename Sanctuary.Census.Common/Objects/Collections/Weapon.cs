using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents weapon data.
/// </summary>
/// <param name="WeaponId">The ID of the weapon.</param>
/// <param name="WeaponGroupId">The ID of the group that the weapon belongs to.</param>
/// <param name="EquipMs">The time in milliseconds that it takes to equip this weapon.</param>
/// <param name="UnequipMs">The time in milliseconds that it takes to unequip this weapon.</param>
/// <param name="ToIronSightsMs">The time in milliseconds that it takes to scope into the iron sights of this weapon.</param>
/// <param name="FromIronSightsMs">The time in milliseconds that it takes to scope out of the iron sights of this weapon.</param>
/// <param name="ToIronSightsAnimMs">The time in milliseconds that iron-sight scope-in animation takes for this weapon.</param>
/// <param name="FromIronSightsAnimMs">The time in milliseconds that iron-sight scope-out animation takes for this weapon.</param>
/// <param name="SprintRecoveryMs">The time in milliseconds that it takes for this weapon to begin resetting its cone-of-fire after sprinting.</param>
/// <param name="NextUseDelayMs">The minimum time in milliseconds between uses of this weapon.</param>
/// <param name="TurnModifier">The turn rate modifier of this weapon.</param>
/// <param name="MoveModifier">The turn speed modifier of this weapon.</param>
/// <param name="HeatBleedOffRate">The rate at which heat bleeds off this weapon.</param>
/// <param name="HeatOverheatPenaltyMs">The time in milliseconds that this weapon takes to begin bleeding heat, after overheating it.</param>
/// <param name="RangeDescription">The description of the range category that this weapon belongs to.</param>
/// <param name="MeleeDetectWidth">The width of the melee bounding box of this weapon.</param>
/// <param name="MeleeDetectHeight">The height of the melee bounding box of this weapon.</param>
/// <param name="AnimationWieldTypeName">The name of this weapon's animation type.</param>
/// <param name="MinViewPitch">The minimum view pitch that this weapon can be used at.</param>
/// <param name="MaxViewPitch">The maximum view pitch that this weapon can be used at.</param>
[Collection]
public record Weapon
(
    [property: JoinKey] uint WeaponId,
    [property: JoinKey] uint? WeaponGroupId,
    ushort EquipMs,
    ushort UnequipMs,
    ushort ToIronSightsMs,
    ushort FromIronSightsMs,
    ushort ToIronSightsAnimMs,
    ushort FromIronSightsAnimMs,
    ushort SprintRecoveryMs,
    uint NextUseDelayMs,
    decimal TurnModifier,
    decimal MoveModifier,
    ushort? HeatBleedOffRate,
    ushort? HeatOverheatPenaltyMs,
    LocaleString? RangeDescription,
    decimal? MeleeDetectWidth,
    decimal? MeleeDetectHeight,
    string? AnimationWieldTypeName,
    decimal? MinViewPitch,
    decimal? MaxViewPitch
) : ISanctuaryCollection;

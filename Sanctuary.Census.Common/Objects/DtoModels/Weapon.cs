using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.DtoModels;

/// <summary>
/// Represents weapon data.
/// </summary>
/// <param name="WeaponID">The ID of the weapon.</param>
/// <param name="WeaponGroupID">The ID of the group that the weapon belongs to.</param>
/// <param name="EquipMS">The time in milliseconds that it takes to equip this weapon.</param>
/// <param name="UnequipMS">The time in milliseconds that it takes to unequip this weapon.</param>
/// <param name="ToIronSightsMS">The time in milliseconds that it takes to scope into the iron sights of this weapon.</param>
/// <param name="FromIronSightsMS">The time in milliseconds that it takes to scope out of the iron sights of this weapon.</param>
/// <param name="ToIronSightsAnimMS">The time in milliseconds that iron-sight scope-in animation takes for this weapon.</param>
/// <param name="FromIronSightsAnimMS">The time in milliseconds that iron-sight scope-out animation takes for this weapon.</param>
/// <param name="SprintRecoveryMS">The time in milliseconds that it takes for this weapon to begin resetting its cone-of-fire after sprinting.</param>
/// <param name="NextUseDelayMS">The minimum time in milliseconds between uses of this weapon.</param>
/// <param name="TurnModifier">The turn rate modifier of this weapon.</param>
/// <param name="MoveModifier">The turn speed modifier of this weapon.</param>
/// <param name="HeatBleedOffRate">The rate at which heat bleeds off this weapon.</param>
/// <param name="HeatOverheatPenaltyMS">The time in milliseconds that this weapon takes to begin bleeding heat, after overheating it.</param>
/// <param name="RangeDescription">The description of the range category that this weapon belongs to.</param>
/// <param name="MeleeDetectWidth">The width of the melee bounding box of this weapon.</param>
/// <param name="MeleeDetectHeight">The height of the melee bounding box of this weapon.</param>
/// <param name="AnimationWieldTypeName">The name of this weapon's animation type.</param>
/// <param name="MinViewPitch">The minimum view pitch that this weapon can be used at.</param>
/// <param name="MaxViewPitch">The maximum view pitch that this weapon can be used at.</param>
public record Weapon
(
    uint WeaponID,
    uint? WeaponGroupID,
    ushort EquipMS,
    ushort UnequipMS,
    ushort ToIronSightsMS,
    ushort FromIronSightsMS,
    ushort ToIronSightsAnimMS,
    ushort FromIronSightsAnimMS,
    ushort SprintRecoveryMS,
    uint NextUseDelayMS,
    float TurnModifier,
    float MoveModifier,
    ushort? HeatBleedOffRate,
    ushort? HeatOverheatPenaltyMS,
    LocaleString? RangeDescription,
    float? MeleeDetectWidth,
    float? MeleeDetectHeight,
    string? AnimationWieldTypeName,
    float? MinViewPitch,
    float? MaxViewPitch
)
{
    /// <summary>
    /// Gets the default <see cref="Item"/>.
    /// </summary>
    public static Weapon Default => new
    (
        0,
        null,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
    );
}

using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.DtoModels;

public record Weapon
(
    uint WeaponID,
    uint WeaponGroupID,
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
    ushort HeadBleedOffRate,
    ushort HeadOverheatPenaltyMS,
    LocaleString RangeDescription,
    float MeleeDetectWidth,
    float MeleeDetectHeight,
    string AnimationWieldTypeName,
    float MinViewPitch,
    float MaxViewPitch
)
{
    /// <summary>
    /// Gets the default <see cref="Item"/>.
    /// </summary>
    public static Weapon Default => new
    (
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
        0,
        0,
        0,
        0,
        LocaleString.Default,
        0,
        0,
        string.Empty,
        0,
        0
    );
}

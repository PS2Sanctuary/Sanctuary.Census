using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a skill.
/// </summary>
/// <param name="SkillID">The ID of the skill.</param>
/// <param name="Name">The name of the skill.</param>
/// <param name="Description">The description of the skill.</param>
/// <param name="SkillSetID">The ID of the skill set that the skill belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillCategoryID">The ID of the skill category that the skill belongs to.</param>
/// <param name="SkillCategoryIndex">Unknown.</param>
/// <param name="SkillLineID">The ID of the skill line that the skill belongs to.</param>
/// <param name="SkillLineIndex">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="GrantAbilityLineID">The ID of the ability line granted by the skill.</param>
/// <param name="GrantAbilityLineIndex">Unknown.</param>
/// <param name="GrantItemID">The ID of the item granted by the skill.</param>
/// <param name="RewardSetID">The ID of the reward set granted by the skill.</param>
/// <param name="IsAutoGranted">Indicates whether the skill is auto-granted.</param>
/// <param name="IsVisible">Indicates whether the skill is visible to the client.</param>
/// <param name="CurrencyID">The ID of the currency used to purchase the skill.</param>
/// <param name="ImageSetID">The ID of the skill's image set.</param>
/// <param name="ImageID">The ID of the skill's default image.</param>
/// <param name="ImagePath">The relative path to the skill's default image.</param>
[Collection]
public record Skill
(
    [property: JoinKey] uint SkillID,
    LocaleString? Name,
    LocaleString? Description,
    [property: JoinKey] uint? SkillSetID,
    ushort? SkillSetIndex,
    [property: JoinKey] uint? SkillCategoryID,
    ushort? SkillCategoryIndex,
    [property: JoinKey] uint? SkillLineID,
    ushort? SkillLineIndex,
    uint SkillPoints,
    uint? GrantAbilityLineID,
    ushort? GrantAbilityLineIndex,
    uint? GrantItemID,
    uint? RewardSetID,
    bool IsAutoGranted,
    bool IsVisible,
    uint CurrencyID,
    [property: JoinKey] uint? ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;

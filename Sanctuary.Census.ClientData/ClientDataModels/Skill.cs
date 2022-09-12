using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client skill data.
/// </summary>
/// <param name="ID">The ID of the skill.</param>
/// <param name="NameID">The locale ID of the skill's name.</param>
/// <param name="DescriptionID">The locale ID of the skill's description.</param>
/// <param name="IconID">The ID of the skill's image set.</param>
/// <param name="SkillSetID">The ID of the <see cref="SkillSet"/> that the skill belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillCategoryIdEx">The ID of the <see cref="SkillCategory"/> that the skill belongs to.</param>
/// <param name="SkillCategoryIndexEx">Unknown.</param>
/// <param name="SkillLineID">The ID of the <see cref="SkillLine"/> that the skill belongs to.</param>
/// <param name="SkillLineIndex">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="ReqSetID">The requirement set that must be met to use the skill.</param>
/// <param name="ClientReqSetID">The client requirement set that must be met to use the skill.</param>
/// <param name="ClientReqSetDescID">The locale ID of the client requirement set's description.</param>
/// <param name="GrantAbilityLineID">The ID of the ability line granted by the skill.</param>
/// <param name="GrantAbilityLineIndex">Unknown.</param>
/// <param name="GrantItemID">The ID of the item granted by the skill.</param>
/// <param name="GrantItemLineID">The ID of the item line granted by the skill.</param>
/// <param name="GrantItemLineIndex">Unknown.</param>
/// <param name="GrantRewardSetID">The ID of the reward set granted by the skill.</param>
/// <param name="FlagAutoGrant">Indicates that the skill is auto-granted.</param>
/// <param name="FlagIsVisible">Indicates whether the skill is visible to the client.</param>
/// <param name="CurrencyID">The ID of the currency that the skill can be purchased with.</param>
[Datasheet]
public partial record Skill
(
    uint ID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint SkillSetID,
    ushort SkillSetIndex,
    uint SkillCategoryIdEx,
    ushort SkillCategoryIndexEx,
    uint SkillLineID,
    ushort SkillLineIndex,
    uint SkillPoints,
    uint ReqSetID,
    uint ClientReqSetID,
    uint ClientReqSetDescID,
    uint GrantAbilityLineID,
    ushort GrantAbilityLineIndex,
    uint GrantItemID,
    uint GrantItemLineID,
    ushort GrantItemLineIndex,
    uint GrantRewardSetID,
    bool FlagAutoGrant,
    bool FlagIsVisible,
    uint CurrencyID
);

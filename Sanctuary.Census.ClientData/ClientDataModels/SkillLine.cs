using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client skill line data.
/// </summary>
/// <param name="ID">The ID of the skill line.</param>
/// <param name="NameID">The locale ID of the line's name.</param>
/// <param name="DescriptionID">The locale ID of the line's description.</param>
/// <param name="IconID">The ID of the line's image set.</param>
/// <param name="SkillSetID">The ID of the <see cref="SkillSet"/> that the line belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillCategoryIdEx">The ID of the <see cref="SkillCategory"/> that the line belongs to.</param>
/// <param name="SkillCategoryIndexEx">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="ReqSetID">The requirement set that must be met to use the line.</param>
/// <param name="ClientReqSetID">The client requirement set that must be met to use the line.</param>
/// <param name="ClientReqSetDescID">The locale ID of the client requirement set's description.</param>
/// <param name="VisibilityReqSetID">The requirement set that must be met for the client to display the line.</param>
/// <param name="FlagIsDeprecated">Unknown.</param>
/// <param name="FlagOmit">Unknown.</param>
/// <param name="DeprecationStringID">The locale ID of the line's deprecation reason.</param>
/// <param name="ScheduleID">Unknown.</param>
/// <param name="ScheduleStartSec">Unknown.</param>
/// <param name="FlagIsPs4Passive">Whether the line is a passive perk on PS4.</param>
/// <param name="Ps4PassiveFactionID">Unknown.</param>
[Datasheet]
public partial record SkillLine
(
    uint ID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint SkillSetID,
    ushort SkillSetIndex,
    uint SkillCategoryIdEx,
    ushort SkillCategoryIndexEx,
    uint SkillPoints,
    uint ReqSetID,
    uint ClientReqSetID,
    uint ClientReqSetDescID,
    uint VisibilityReqSetID,
    uint FlagIsDeprecated,
    bool FlagOmit,
    uint DeprecationStringID,
    uint ScheduleID,
    string ScheduleStartSec,
    bool FlagIsPs4Passive,
    byte Ps4PassiveFactionID
);

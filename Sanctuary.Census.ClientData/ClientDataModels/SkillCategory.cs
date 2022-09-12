using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents skill category client data.
/// </summary>
/// <param name="ID">The ID of the skill category.</param>
/// <param name="NameID">The locale ID of the category's name.</param>
/// <param name="DescriptionID">The locale ID of the category's description.</param>
/// <param name="IconID">The ID of the category's image set.</param>
/// <param name="SkillSetID">The ID of the <see cref="SkillSet"/> that the category belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="ReqSetID">The requirement set that must be met to use the skill category.</param>
/// <param name="ClientReqSetID">The client requirement set that must be met to use the skill category.</param>
/// <param name="ClientReqSetDescID">The locale ID of the client requirement set's description.</param>
/// <param name="ScheduleID">Unknown.</param>
/// <param name="ScheduleStartSec">Unknown.</param>
[Datasheet]
public partial record SkillCategory
(
    uint ID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint SkillSetID,
    ushort SkillSetIndex,
    uint SkillPoints,
    uint ReqSetID,
    uint ClientReqSetID,
    uint ClientReqSetDescID,
    uint ScheduleID,
    string ScheduleStartSec
);

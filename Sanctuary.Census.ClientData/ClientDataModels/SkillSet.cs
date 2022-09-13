using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client skill set data.
/// </summary>
/// <param name="ID">The ID of the skill set.</param>
/// <param name="NameID">The locale ID of the set's name.</param>
/// <param name="DescriptionID">The locale ID of the set's description.</param>
/// <param name="IconID">The ID of the set's image set.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="ReqSetID">Unknown.</param>
/// <param name="ClientReqSetID">Unknown.</param>
/// <param name="ClientReqSetDescID">Unknown.</param>
/// <param name="VisibilityReqSetID">The requirement set that must be met for the skill set to be visible to the client.</param>
/// <param name="RequiredItemID">Unknown.</param>
/// <param name="ScheduleID">Unknown.</param>
/// <param name="ScheduleStartSec">Unknown.</param>
[Datasheet]
public partial record SkillSet
(
    uint ID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint SkillPoints,
    uint ReqSetID,
    uint ClientReqSetID,
    uint ClientReqSetDescID,
    uint VisibilityReqSetID,
    uint RequiredItemID,
    uint ScheduleID,
    string ScheduleStartSec
);

using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a skill line.
/// </summary>
/// <param name="SkillLineID">The ID of the skill line.</param>
/// <param name="SkillSetID">The ID of the skill set that the line belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillCategoryID">The ID of the skill category that the line belongs to.</param>
/// <param name="SkillCategoryIndex">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="Name">The name of the skill line.</param>
/// <param name="Description">The description of the skill line.</param>
/// <param name="ImageSetID">The ID of the line's image set.</param>
/// <param name="ImageID">The ID of the line's default image.</param>
/// <param name="ImagePath">The relative path to the line's default image.</param>
[Collection]
public record SkillLine
(
    [property: JoinKey] uint SkillLineID,
    [property: JoinKey] uint? SkillSetID,
    ushort? SkillSetIndex,
    [property: JoinKey] uint? SkillCategoryID,
    ushort? SkillCategoryIndex,
    uint SkillPoints,
    LocaleString? Name,
    LocaleString? Description,
    [property: JoinKey] uint? ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;

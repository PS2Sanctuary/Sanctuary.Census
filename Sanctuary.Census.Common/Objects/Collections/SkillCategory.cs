using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a skill category.
/// </summary>
/// <param name="SkillCategoryID">The ID of the category.</param>
/// <param name="SkillSetID">The ID of the skill set that the category belongs to.</param>
/// <param name="SkillSetIndex">Unknown.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="Description">The description of the category.</param>
/// <param name="ImageSetID">The ID of the category's image set.</param>
/// <param name="ImageID">The ID of the category's default image.</param>
/// <param name="ImagePath">The relative path to the category's default image.</param>
[Collection]
public record SkillCategory
(
    uint SkillCategoryID,
    uint SkillSetID,
    ushort SkillSetIndex,
    uint SkillPoints,
    LocaleString? Name,
    LocaleString? Description,
    uint ImageSetID,
    uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;

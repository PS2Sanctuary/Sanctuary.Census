using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a skill set.
/// </summary>
/// <param name="SkillSetID">The ID of the skill set.</param>
/// <param name="SkillPoints">Unknown.</param>
/// <param name="RequiredItemID">Unknown.</param>
/// <param name="Name">The name of the skill set.</param>
/// <param name="Description">The description of the skill set.</param>
/// <param name="ImageSetID">The ID of the skill set's image set.</param>
/// <param name="ImageID">The ID of the set's default image.</param>
/// <param name="ImagePath">The relative path to the set's default image.</param>
[Collection]
public record SkillSet
(
    uint SkillSetID,
    uint SkillPoints,
    uint? RequiredItemID,
    LocaleString? Name,
    LocaleString? Description,
    uint ImageSetID,
    uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;

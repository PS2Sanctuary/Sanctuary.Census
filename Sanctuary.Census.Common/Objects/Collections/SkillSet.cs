using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

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

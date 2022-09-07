using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a directive.
/// </summary>
/// <param name="DirectiveID">The ID of the directive.</param>
/// <param name="DirectiveTreeID">The ID of the tree that the directive is nested under.</param>
/// <param name="DirectiveTierID">The ID of the tier that the directive belongs to.</param>
/// <param name="Name">The name of the directive.</param>
/// <param name="Description">The description of the directive.</param>
/// <param name="ImageSetID">The ID of the directive's image set.</param>
/// <param name="ImageID">The ID of the directive's default image.</param>
/// <param name="ImagePath">The relative path to the directive's default image.</param>
/// <param name="ObjectiveParam1">Unknown.</param>
[Collection]
[Description("Represents an objective of a directive tier.")]
public record Directive
(
    uint DirectiveID,
    uint DirectiveTreeID,
    uint DirectiveTierID,
    LocaleString? Name,
    LocaleString? Description,
    uint ImageSetID,
    uint? ImageID,
    string? ImagePath,
    uint ObjectiveParam1
) : ISanctuaryCollection;

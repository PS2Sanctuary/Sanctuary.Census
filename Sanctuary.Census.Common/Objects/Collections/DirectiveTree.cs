using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a directive tree.
/// </summary>
/// <param name="DirectiveTreeID">The ID of the tree.</param>
/// <param name="DirectiveTreeCategoryID">The ID of the <see cref="DirectiveTreeCategory"/> that the tree belongs to.</param>
/// <param name="FactionIds">The IDs of the factions that this tree is available on.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="Description">The description of the category.</param>
/// <param name="ImageSetID">The ID of the tree's image set.</param>
/// <param name="ImageID">The ID of the tree's default image.</param>
/// <param name="ImagePath">The relative path to the tree's default image.</param>
[Collection]
[Description("Represents a single directive as a whole, e.g. Exceptional III. Generally linked to four children directive_tier entries")]
public record DirectiveTree
(
    [property: JoinKey] uint DirectiveTreeID,
    [property: JoinKey] uint DirectiveTreeCategoryID,
    ValueEqualityList<uint> FactionIds,
    LocaleString Name,
    LocaleString? Description,
    [property: JoinKey] uint ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;

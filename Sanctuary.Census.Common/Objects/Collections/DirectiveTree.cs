using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a directive tree.
/// </summary>
/// <param name="DirectiveTreeID">The ID of the tree.</param>
/// <param name="DirectiveTreeCategoryID">The ID of the <see cref="DirectiveTreeCategory"/> that the tree belongs to.</param>
/// <param name="FactionID">The ID of the faction that this directive tree is available on.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="Description">The description of the category.</param>
/// <param name="ImageSetID">The ID of the tree's image set.</param>
/// <param name="ImageID">The ID of the tree's default image.</param>
/// <param name="ImagePath">The relative path to the tree's default image.</param>
[Collection]
public record DirectiveTree
(
    [property: Key] uint DirectiveTreeID,
    [property: Key] uint DirectiveTreeCategoryID,
    [property: Key] uint FactionID,
    LocaleString Name,
    LocaleString? Description,
    uint ImageSetID,
    uint ImageID,
    string ImagePath
) : ISanctuaryCollection;

using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a category for directive trees.
/// </summary>
/// <param name="DirectiveTreeCategoryID">The ID of the category.</param>
/// <param name="FactionID">The ID of the faction that this category is available on.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="DisplayOrder">The order in which the category is displayed in the UI.</param>
[Collection]
public record DirectiveTreeCategory
(
    [property: Key] uint DirectiveTreeCategoryID,
    [property: Key] uint FactionID,
    LocaleString Name,
    byte DisplayOrder
) : ISanctuaryCollection;

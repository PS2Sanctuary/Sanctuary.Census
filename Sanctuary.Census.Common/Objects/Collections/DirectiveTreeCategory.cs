using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a category for directive trees.
/// </summary>
/// <param name="DirectiveTreeCategoryID">The ID of the category.</param>
/// <param name="FactionIds">The IDs of the factions that this tree is available on.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="DisplayOrder">The order in which the category is displayed in the UI.</param>
[Collection]
public record DirectiveTreeCategory
(
    [property: Key] uint DirectiveTreeCategoryID,
    ValueEqualityList<uint> FactionIds,
    LocaleString Name,
    byte DisplayOrder
) : ISanctuaryCollection;

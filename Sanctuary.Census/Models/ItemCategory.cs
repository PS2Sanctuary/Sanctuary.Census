using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Models;

/// <summary>
/// Represents item category data.
/// </summary>
/// <param name="ItemCategoryID">The ID of the item category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="ParentCategoryIds">
/// The IDs of this category's parents, ordered such that the immediate
/// parent/s are the first items in the collection.
/// </param>
public record ItemCategory
(
    uint ItemCategoryID,
    LocaleString Name,
    uint[]? ParentCategoryIds
);

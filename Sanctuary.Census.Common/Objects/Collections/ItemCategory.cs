using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents item category data.
/// </summary>
/// <param name="ItemCategoryID">The ID of the item category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="ParentCategoryIds">
/// The IDs of this category's parents, ordered such that the immediate
/// parent/s are the first items in the collection.
/// </param>
[Collection]
public record ItemCategory
(
    [property: Key] uint ItemCategoryID,
    LocaleString Name,
    uint[]? ParentCategoryIds
)
{
    /// <inheritdoc />
    public virtual bool Equals(ItemCategory? obj)
        => obj is not null && GetHashCode() == obj.GetHashCode();

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ItemCategoryID);
        hash.Add(Name);

        if (ParentCategoryIds is not null)
        {
            foreach (uint value in ParentCategoryIds)
                hash.Add(value);
        }

        return hash.ToHashCode();
    }
}

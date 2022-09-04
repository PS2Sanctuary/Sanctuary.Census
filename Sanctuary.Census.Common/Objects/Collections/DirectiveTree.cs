using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
public record DirectiveTree
(
    [property: Key] uint DirectiveTreeID,
    [property: Key] uint DirectiveTreeCategoryID,
    List<uint> FactionIds,
    LocaleString Name,
    LocaleString? Description,
    uint ImageSetID,
    uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection
{
    /// <inheritdoc />
    public virtual bool Equals(DirectiveTree? other)
        => other is not null
           && other.DirectiveTreeID.Equals(DirectiveTreeID)
           && other.DirectiveTreeCategoryID.Equals(DirectiveTreeCategoryID)
           && other.FactionIds.SequenceEqual(FactionIds)
           && other.Name.Equals(Name)
           && other.Description?.Equals(Description) == true
           && other.ImageSetID.Equals(ImageSetID)
           && other.ImageID.Equals(ImageID)
           && other.ImagePath?.Equals(ImagePath) == true;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(DirectiveTreeID);
        hash.Add(DirectiveTreeCategoryID);
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(ImageSetID);
        hash.Add(ImageID);
        hash.Add(ImagePath);
        foreach (uint faction in FactionIds)
            hash.Add(faction);

        return hash.ToHashCode();
    }
}

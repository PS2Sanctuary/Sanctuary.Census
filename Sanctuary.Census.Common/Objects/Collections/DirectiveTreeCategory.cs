﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
    List<uint> FactionIds,
    LocaleString Name,
    byte DisplayOrder
) : ISanctuaryCollection
{
    /// <inheritdoc />
    public virtual bool Equals(DirectiveTreeCategory? other)
        => other is not null
           && other.DirectiveTreeCategoryID == DirectiveTreeCategoryID
           && other.FactionIds.SequenceEqual(FactionIds)
           && other.Name == Name
           && other.DisplayOrder == DisplayOrder;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(DirectiveTreeCategoryID);
        hash.Add(Name);
        hash.Add(DisplayOrder);
        foreach (uint faction in FactionIds)
            hash.Add(faction);

        return hash.ToHashCode();
    }
}

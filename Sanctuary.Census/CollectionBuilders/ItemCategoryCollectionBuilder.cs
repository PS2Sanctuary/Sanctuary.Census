using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Linq;
using MItemCategory = Sanctuary.Census.Models.Collections.ItemCategory;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Models.Collections.ItemCategory"/> collection.
/// </summary>
public class ItemCategoryCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (serverDataCache.ItemCategories is null)
            throw new MissingCacheDataException(typeof(ItemCategories));

        Dictionary<uint, List<uint>> parentTree = new();
        foreach (CategoryHierarchy h in serverDataCache.ItemCategories.Hierarchies)
        {
            parentTree.TryAdd(h.ChildCategoryID, new List<uint>());
            parentTree[h.ChildCategoryID].Add(h.ParentCategoryID);
        }

        Dictionary<uint, MItemCategory> builtCategories = new();
        foreach (ItemCategory category in serverDataCache.ItemCategories.Categories.OrderBy(ic => ic.ItemCategoryID))
        {
            List<uint> parents = new();
            Queue<uint> currIDs = new();
            currIDs.Enqueue(category.ItemCategoryID);

            while (currIDs.TryDequeue(out uint checkForParent))
            {
                if (!parentTree.TryGetValue(checkForParent, out List<uint>? sp))
                    continue;

                parents.AddRange(sp);
                foreach (uint val in sp)
                    currIDs.Enqueue(val);
            }

            localeService.TryGetLocaleString(category.NameID, out LocaleString? name);

            MItemCategory built = new
            (
                category.ItemCategoryID,
                name!,
                parents.Count == 0 ? null : parents.ToArray()
            );
            builtCategories.TryAdd(built.ItemCategoryID, built);
        }

        context.ItemCategories = builtCategories;
    }
}

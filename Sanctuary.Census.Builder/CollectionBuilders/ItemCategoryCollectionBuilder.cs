using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MItemCategory = Sanctuary.Census.Common.Objects.Collections.ItemCategory;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Common.Objects.Collections.ItemCategory"/> collection.
/// </summary>
public class ItemCategoryCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCategoryCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public ItemCategoryCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_serverDataCache.ItemCategories is null)
            throw new MissingCacheDataException(typeof(ItemCategories));

        Dictionary<uint, List<uint>> parentTree = new();
        foreach (CategoryHierarchy h in _serverDataCache.ItemCategories.Hierarchies)
        {
            parentTree.TryAdd(h.ChildCategoryID, new List<uint>());
            parentTree[h.ChildCategoryID].Add(h.ParentCategoryID);
        }

        Dictionary<uint, MItemCategory> builtCategories = new();
        foreach (ItemCategory category in _serverDataCache.ItemCategories.Categories.OrderBy(ic => ic.ItemCategoryID))
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

            _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);

            MItemCategory built = new
            (
                category.ItemCategoryID,
                name!,
                parents.Count == 0 ? null : new ValueEqualityList<uint>(parents)
            );
            builtCategories.TryAdd(built.ItemCategoryID, built);
        }

        await dbContext.UpsertCollectionAsync(builtCategories.Values, ct).ConfigureAwait(false);
    }
}

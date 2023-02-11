using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="ItemToItemLine"/> collection.
/// </summary>
public class ItemToItemLineCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemToItemLineCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public ItemToItemLineCollectionBuilder(IClientDataCacheService clientDataCache)
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.ItemLineMembers is null)
            throw new MissingCacheDataException(typeof(ItemLineMember));

        List<ItemToItemLine> builtLineMembers = _clientDataCache.ItemLineMembers
            .Select(element => new ItemToItemLine
            (
                element.ItemId,
                element.ItemLineId,
                element.ItemLineIndex
            ))
            .ToList();

        await dbContext.UpsertCollectionAsync(builtLineMembers, ct).ConfigureAwait(false);
    }
}

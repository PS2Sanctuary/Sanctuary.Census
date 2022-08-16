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
/// Builds the <see cref="ItemToWeapon"/> collection.
/// </summary>
public class ItemToWeaponCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemToWeaponCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public ItemToWeaponCollectionBuilder
    (
        IClientDataCacheService clientDataCache
    )
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
        if (_clientDataCache.ClientItemDatasheetDatas is null)
            throw new MissingCacheDataException(typeof(ClientItemDatasheetData));

        IEnumerable<ItemToWeapon> builtItemsToWeapon = _clientDataCache.ClientItemDatasheetDatas
            .Select(i => new ItemToWeapon(i.ItemID, i.WeaponID));

        await dbContext.UpsertItemsToWeaponsAsync(builtItemsToWeapon, ct);
    }
}

using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using System.Collections.Generic;
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
        if (_clientDataCache.ClientItemDefinitions is null)
            throw new MissingCacheDataException(typeof(ClientItemDefinition));

        if (_clientDataCache.ClientItemDatasheetDatas is null)
            throw new MissingCacheDataException(typeof(ClientItemDatasheetData));

        Dictionary<uint, ItemToWeapon> builtItemsToWeapons = new();

        foreach (ClientItemDefinition itemDef in _clientDataCache.ClientItemDefinitions)
        {
            if (itemDef.CodeFactoryName is not "Weapon")
                continue;

            if (itemDef.Param1 is 0)
                continue;

            builtItemsToWeapons.TryAdd
            (
                itemDef.ID,
                new ItemToWeapon(itemDef.ID, (uint)itemDef.Param1)
            );
        }

        foreach (ClientItemDatasheetData element in _clientDataCache.ClientItemDatasheetDatas)
        {
            if (element.WeaponID is 0)
                continue;

            builtItemsToWeapons.TryAdd
            (
                element.ItemID,
                new ItemToWeapon(element.ItemID, element.WeaponID)
            );
        }

        await dbContext.UpsertCollectionAsync(builtItemsToWeapons.Values, ct).ConfigureAwait(false);
    }
}

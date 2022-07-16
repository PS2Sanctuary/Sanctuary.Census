using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="ItemToWeapon"/> collection.
/// </summary>
public class ItemToWeaponCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        CollectionsContext context
    )
    {
        if (clientDataCache.ClientItemDatasheetDatas.Count == 0)
            throw new MissingCacheDataException(typeof(ClientItemDatasheetData));

        Dictionary<uint, ItemToWeapon> builtItemsToWeapon = new();
        foreach (ClientItemDatasheetData cidd in clientDataCache.ClientItemDatasheetDatas)
        {
            ItemToWeapon built = new(cidd.ItemID, cidd.WeaponID);
            builtItemsToWeapon.Add(built.ItemId, built);
        }

        context.ItemsToWeapon = builtItemsToWeapon;
    }
}

using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="ItemToWeapon"/> collection.
/// </summary>
public class ItemToWeaponCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public async Task BuildAsync
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (clientDataCache.ClientItemDatasheetDatas.Count == 0)
            throw new MissingCacheDataException(typeof(ClientItemDatasheetData));

        IEnumerable<ItemToWeapon> builtItemsToWeapon = clientDataCache.ClientItemDatasheetDatas
            .Select(i => new ItemToWeapon(i.ItemID, i.WeaponID));

        await dbContext.UpsertItemsToWeaponsAsync(builtItemsToWeapon, ct);
    }
}

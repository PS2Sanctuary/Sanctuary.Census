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
/// Builds the <see cref="WeaponToAttachment"/> collection.
/// </summary>
public class WeaponToAttachmentCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponToAttachmentCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public WeaponToAttachmentCollectionBuilder
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
        if (_clientDataCache.LoadoutAttachmentGroupMaps is null)
            throw new MissingCacheDataException(typeof(LoadoutAttachmentGroupMap));

        List<WeaponToAttachment> builtMaps = new();
        foreach (LoadoutAttachmentGroupMap map in _clientDataCache.LoadoutAttachmentGroupMaps)
        {
            WeaponToAttachment built = new
            (
                map.ItemTypeGroupID,
                map.AttachmentID,
                map.ItemID
            );
            builtMaps.Add(built);
        }

        await dbContext.UpsertWeaponToAttachmentsAsync(builtMaps, ct);
    }
}

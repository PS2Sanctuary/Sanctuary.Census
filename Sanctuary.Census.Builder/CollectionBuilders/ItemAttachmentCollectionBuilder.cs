using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.InGamePurchase;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="ItemAttachment"/> collection.
/// </summary>
public class ItemAttachmentCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAttachmentCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public ItemAttachmentCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_clientDataCache.ItemLineMembers is null)
            throw new MissingCacheDataException(typeof(ItemLineMember));

        if (_clientDataCache.LoadoutItemAttachments is null)
            throw new MissingCacheDataException(typeof(LoadoutItemAttachment));

        if (_clientDataCache.LoadoutAttachments is null)
            throw new MissingCacheDataException(typeof(LoadoutAttachment));

        if (_serverDataCache.StoreBundles.Count != 4)
            throw new MissingCacheDataException(typeof(StoreBundles));

        HashSet<uint> validItems = new();
        foreach (StoreBundles bundles in _serverDataCache.StoreBundles.Values)
        {
            foreach (StoreBundles_Bundle bundle in bundles.Bundles)
            {
                foreach (StoreBundles_ItemListDetail item in bundle.ItemListDetails)
                    validItems.Add(item.ItemID);
            }
        }

        Dictionary<uint, List<uint>> itemLineToItems = new();
        foreach (ItemLineMember ilm in _clientDataCache.ItemLineMembers)
        {
            if (!itemLineToItems.TryGetValue(ilm.ItemLineId, out List<uint>? items))
            {
                items = new List<uint>();
                itemLineToItems.Add(ilm.ItemLineId, items);
            }
            items.Add(ilm.ItemId);
        }

        Dictionary<uint, List<uint>> itemLineToDefaultAttachments = new();
        foreach (LoadoutItemAttachment lia in _clientDataCache.LoadoutItemAttachments)
        {
            if (!itemLineToDefaultAttachments.TryGetValue(lia.ItemLineId, out List<uint>? defaultAttachments))
            {
                defaultAttachments = new List<uint>();
                itemLineToDefaultAttachments.Add(lia.ItemLineId, defaultAttachments);
            }
            defaultAttachments.Add(lia.AttachmentId);
        }

        Dictionary<uint, List<uint>> groupToAttachments = new();
        Dictionary<uint, LoadoutAttachment> attachmentDetails = new();
        foreach (LoadoutAttachment attachment in _clientDataCache.LoadoutAttachments)
        {
            if (!groupToAttachments.TryGetValue(attachment.GroupId, out List<uint>? groupedAttachments))
            {
                groupedAttachments = new List<uint>();
                groupToAttachments.Add(attachment.GroupId, groupedAttachments);
            }
            groupedAttachments.Add(attachment.Id);
            attachmentDetails.Add(attachment.Id, attachment);
        }

        List<ItemAttachment> builtAttachments = new();
        foreach (ItemLineMember lineMember in _clientDataCache.ItemLineMembers)
        {
            if (!itemLineToDefaultAttachments.TryGetValue(lineMember.ItemLineId, out List<uint>? defaultAttachments))
                continue;

            if (defaultAttachments.Count is 0)
                continue;

            HashSet<uint> allAttachments = new(defaultAttachments);
            foreach (uint element in defaultAttachments)
            {
                if (!attachmentDetails.TryGetValue(element, out LoadoutAttachment? groupMarker))
                    continue;

                if (!groupToAttachments.TryGetValue(groupMarker.GroupId, out List<uint>? groupedAttachments))
                    continue;

                foreach (uint attachment in groupedAttachments)
                    allAttachments.Add(attachment);
            }

            foreach (uint attachment in allAttachments)
            {
                if (!attachmentDetails.TryGetValue(attachment, out LoadoutAttachment? detail))
                    continue;

                if (!itemLineToItems.TryGetValue(detail.ItemLineId, out List<uint>? items))
                    continue;

                bool isDefault = defaultAttachments.Contains(attachment);
                foreach (uint item in items)
                {
                    if (!validItems.Contains(item))
                        continue;

                    builtAttachments.Add(new ItemAttachment
                    (
                        lineMember.ItemId,
                        item,
                        isDefault,
                        detail.FlagRequired
                    ));
                }
            }
        }

        await dbContext.UpsertCollectionAsync(builtAttachments, ct).ConfigureAwait(false);
    }
}

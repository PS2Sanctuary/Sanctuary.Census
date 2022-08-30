using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="VehicleAttachment"/> collection.
/// </summary>
public class VehicleAttachmentCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleAttachmentCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public VehicleAttachmentCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
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

        if (_clientDataCache.ItemVehicles is null)
            throw new MissingCacheDataException(typeof(ItemVehicle));

        if (_clientDataCache.VehicleLoadoutSlotItemClasses is null)
            throw new MissingCacheDataException(typeof(VehicleLoadoutSlotItemClass));

        Dictionary<uint, uint> itemClasses = new();
        foreach (ClientItemDefinition cid in _clientDataCache.ClientItemDefinitions)
            itemClasses.TryAdd(cid.ID, cid.ItemClass);

        Dictionary<uint, Dictionary<uint, uint>> slotBuckets = new();
        foreach (VehicleLoadoutSlotItemClass slotMap in _clientDataCache.VehicleLoadoutSlotItemClasses)
        {
            slotBuckets.TryAdd(slotMap.LoadoutID, new Dictionary<uint, uint>());
            slotBuckets[slotMap.LoadoutID][slotMap.ItemClass] = slotMap.SlotID;
        }

        List<VehicleAttachment> builtAttachments = new();
        foreach (ItemVehicle vehicleAttachment in _clientDataCache.ItemVehicles)
        {
            _localeDataCache.TryGetLocaleString(vehicleAttachment.VehicleNameID, out LocaleString? name);

            uint slotID = 0;

            slotBuckets.TryGetValue(vehicleAttachment.VehicleLoadoutID, out Dictionary<uint, uint>? itemClassToSlotMap);
            bool foundItemClass = itemClasses.TryGetValue(vehicleAttachment.ItemID, out uint itemClass);
            if (foundItemClass)
                itemClassToSlotMap?.TryGetValue(itemClass, out slotID);

            VehicleAttachment built = new
            (
                vehicleAttachment.ItemID,
                vehicleAttachment.VehicleLoadoutID,
                vehicleAttachment.VehicleID,
                vehicleAttachment.FactionID,
                slotID == 0 ? null : slotID,
                name!
            );
            builtAttachments.Add(built);
        }

        await dbContext.UpsertCollectionAsync(builtAttachments, ct).ConfigureAwait(false);
    }
}

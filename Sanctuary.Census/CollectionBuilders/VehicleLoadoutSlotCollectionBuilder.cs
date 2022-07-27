using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicleLoadoutSlot = Sanctuary.Census.Models.Collections.VehicleLoadoutSlot;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicleLoadoutSlot"/> collection.
/// </summary>
public class VehicleLoadoutSlotCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleLoadoutSlotCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public VehicleLoadoutSlotCollectionBuilder
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
        if (_clientDataCache.VehicleLoadoutSlots is null)
            throw new MissingCacheDataException(typeof(VehicleLoadoutSlot));

        List<MVehicleLoadoutSlot> builtSlots = new();
        foreach (VehicleLoadoutSlot slot in _clientDataCache.VehicleLoadoutSlots)
        {
            _localeDataCache.TryGetLocaleString(slot.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(slot.DescriptionID, out LocaleString? description);

            MVehicleLoadoutSlot built = new
            (
                slot.LoadoutID,
                slot.SlotID,
                name,
                description,
                slot.IconID == 0 ? null : slot.IconID,
                slot.FlagAutoEquip,
                slot.FlagRequired,
                slot.FlagIsVisible,
                slot.SlotUITag
            );
            builtSlots.Add(built);
        }

        await dbContext.UpsertVehicleLoadoutSlotsAsync(builtSlots, ct);
    }
}

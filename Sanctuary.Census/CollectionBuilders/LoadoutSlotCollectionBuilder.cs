using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MLoadoutSlot = Sanctuary.Census.Models.Collections.LoadoutSlot;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MLoadoutSlot"/> collection.
/// </summary>
public class LoadoutSlotCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutSlotCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public LoadoutSlotCollectionBuilder
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
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.LoadoutSlots is null)
            throw new MissingCacheDataException(typeof(LoadoutSlot));

        List<MLoadoutSlot> builtSlots = new();
        foreach (LoadoutSlot slot in _clientDataCache.LoadoutSlots)
        {
            _localeDataCache.TryGetLocaleString(slot.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(slot.DescriptionID, out LocaleString? description);

            MLoadoutSlot built = new
            (
                slot.LoadoutID,
                slot.SlotID,
                name,
                description,
                slot.FlagAutoEquip,
                slot.FlagRequired,
                slot.FlagIsVisible,
                slot.SlotUITag
            );
            builtSlots.Add(built);
        }

        await dbContext.UpsertLoadoutSlotsAsync(builtSlots, ct);
    }
}

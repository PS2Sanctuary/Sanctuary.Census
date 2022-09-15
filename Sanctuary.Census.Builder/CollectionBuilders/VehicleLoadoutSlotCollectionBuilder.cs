using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicleLoadoutSlot = Sanctuary.Census.Common.Objects.Collections.VehicleLoadoutSlot;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicleLoadoutSlot"/> collection.
/// </summary>
public class VehicleLoadoutSlotCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleLoadoutSlotCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public VehicleLoadoutSlotCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _imageSetHelper = imageSetHelper;
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
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(slot.IconID, out uint defaultImage);

            MVehicleLoadoutSlot built = new
            (
                slot.LoadoutID,
                slot.SlotID,
                name,
                description,
                slot.FlagAutoEquip,
                slot.FlagRequired,
                slot.FlagIsVisible,
                slot.SlotUITag,
                slot.IconID == 0 ? null : slot.IconID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            );
            builtSlots.Add(built);
        }

        await dbContext.UpsertCollectionAsync(builtSlots, ct).ConfigureAwait(false);
    }
}
